using System.IO.Pipes;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Tiempito.Daemon.Application.Commands;
using Tiempito.Daemon.Application.Notifications;
using Tiempito.Daemon.Server.Configuration;
using Tiempito.IPC.Abstractions;
using Tiempito.IPC.Models;
using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.Daemon.Tests.Server;

/// <summary>
/// Unit tests for <see cref="ServerTests"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature ", "Server")]
public sealed class ServerTests : IDisposable
{
    private const string PipeName = "test";

    private readonly MockRepository _mockRepository;
    private readonly NamedPipeServerStream _pipeServer;
    private readonly Mock<IStandardOutQueueReader> _stdOutQueueReaderMock;
    private readonly Mock<ICommandDispatcher> _commandDispatcherMock;
    private readonly Mock<IMessageWriter> _messageWriterMock;
    private readonly Mock<IMessageReader> _messageReaderMock;
    private readonly NamedPipeClientStream _pipeClient;
    private readonly SemaphoreSlim _semaphoreSlim;
    private readonly Daemon.Server.Server _server;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerTests"/> class.
    /// </summary>
    public ServerTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Loose);
        var loggerMock = _mockRepository.Create<ILogger<Daemon.Server.Server>>();
        var pipeOptions = _mockRepository.Create<IOptions<PipeConfig>>();
        _pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        _stdOutQueueReaderMock = _mockRepository.Create<IStandardOutQueueReader>();
        _commandDispatcherMock = _mockRepository.Create<ICommandDispatcher>();
        _messageWriterMock = _mockRepository.Create<IMessageWriter>();
        _messageReaderMock = _mockRepository.Create<IMessageReader>();
        pipeOptions.Setup(m => m.Value).Returns(new PipeConfig());
        _pipeClient = new NamedPipeClientStream(PipeName);
        _semaphoreSlim = new SemaphoreSlim(0);
        _server = new Daemon.Server.Server(
            loggerMock.Object,
            pipeOptions.Object,
            _pipeServer,
            _stdOutQueueReaderMock.Object,
            _commandDispatcherMock.Object,
            _messageWriterMock.Object,
            _messageReaderMock.Object);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
        _pipeServer.Dispose();
        _pipeClient.Dispose();
        _semaphoreSlim.Dispose();
    }

    /// <summary>
    /// Tests that <see cref="Daemon.Server.Server.StartAsync"/> waits for pipe connections to be established.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StartAsync_should_WaitForPipeConnections()
    {
        // Act
        await SetupPipeConnectionAsync();

        // Assert
        Assert.True(_pipeClient.IsConnected);
        Assert.True(_pipeServer.IsConnected);
    }

    /// <summary>
    /// Tests that <see cref="Daemon.Server.Server.StopAsync"/> stops the pipe connection and sends a termination message.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StopAsync_should_StopPipeConnection()
    {
        // Arrange
        await SetupPipeConnectionAsync();

        // Act
        await _server.StopAsync(CancellationToken.None);

        // Assert
        Assert.False(_pipeServer.IsConnected);
        _messageWriterMock.Verify(
            m => m.WriteAsync<Message>(_pipeServer, It.IsAny<ConnectionTerminationMessage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that calling <see cref="Daemon.Server.Server.DisposeAsync"/> stops the pipe connection.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisposeAsync_should_StopPipeConnection()
    {
        // Arrange
        await SetupPipeConnectionAsync();

        // Act
        await _server.DisposeAsync();

        // Assert
        Assert.False(_pipeServer.IsConnected);
    }

    /// <summary>
    /// Tests that a received command is dispatched and the expected response is returned.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task It_should_DispatchReceivedCommandAndReturnExpectedResponse()
    {
        // Arrange
        var command = PauseSessionCommand.CreateNew();
        var response = Response.Ok(command.CorrelationId, "Test message");
        _messageReaderMock.Setup(m => m.ReadAsync<Message>(_pipeServer, It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await _semaphoreSlim.WaitAsync();
                return command;
            });
        _commandDispatcherMock.Setup(m => m.DispatchAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        await SetupPipeConnectionAsync();

        // Act
        _semaphoreSlim.Release();
        await WaitUntilAsync(() => _messageWriterMock.Invocations.Count > 0);

        // Assert
        _commandDispatcherMock.Verify(m => m.DispatchAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        _messageWriterMock.Verify(
            m => m.WriteAsync<Message>(_pipeServer, response, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that the server disconnects from the pipe when a <see cref="ConnectionTerminationMessage"/> is received.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task It_should_DisconnectFromPipe_when_ServerReceivesAConnectionTerminationMessage()
    {
        // Arrange
        var message = ConnectionTerminationMessage.CreateNew();
        _messageReaderMock.Setup(m => m.ReadAsync<Message>(_pipeServer, It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await _semaphoreSlim.WaitAsync();
                return message;
            });
        await SetupPipeConnectionAsync();

        // Act
        _semaphoreSlim.Release();
        await WaitUntilAsync(() => !_pipeServer.IsConnected);

        // Assert
        Assert.False(_pipeServer.IsConnected);
    }

    /// <summary>
    /// Tests that messages from the standard output queue reader are sent to the client.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task It_should_SendMessagesToTheClient_when_StdOutQueueReaderHasMessages()
    {
        // Arrange
        var message = Response.Ok(Guid.NewGuid(), "Test message");
        _stdOutQueueReaderMock.Setup(m => m.Reader.WaitToReadAsync(It.IsAny<CancellationToken>())).Returns(
            async () =>
            {
                await _semaphoreSlim.WaitAsync();
                return true;
            });
        _stdOutQueueReaderMock.Setup(m => m.Reader.ReadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(message);
        await SetupPipeConnectionAsync();

        // Act
        _semaphoreSlim.Release();
        await WaitUntilAsync(() => _messageWriterMock.Invocations.Count > 0);

        // Assert
        _messageWriterMock.Verify(
            m => m.WriteAsync<Message>(_pipeServer, message, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 1000, int pollIntervalMs = 10)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(pollIntervalMs);
        }

        if (!condition())
        {
            throw new TimeoutException($"Condition was not met within {timeoutMs}ms.");
        }
    }

    private async Task SetupPipeConnectionAsync()
    {
        await _server.StartAsync(CancellationToken.None);
        await _pipeClient.ConnectAsync();
        await WaitUntilAsync(() => _pipeServer.IsConnected);
    }
}