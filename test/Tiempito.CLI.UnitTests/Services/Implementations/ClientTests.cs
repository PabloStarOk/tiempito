using System.IO.Pipes;

using Moq;

using Tiempito.CLI.Services.Implementations;
using Tiempito.IPC.Abstractions;
using Tiempito.IPC.Models;
using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.CLI.UnitTests.Services.Implementations;

/// <summary>
/// Unit tests for the <see cref="Client"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature ", "CLI")]
public sealed class ClientTests : IDisposable
{
    private static readonly CancellationToken CancellationToken = CancellationToken.None;
    private readonly MockRepository _mockRepository;
    private readonly Mock<IMessageWriter> _messageWriterMock;
    private readonly Mock<IMessageReader> _messageReaderMock;
    private readonly NamedPipeClientStream _pipeClient;
    private readonly NamedPipeServerStream _pipeServer;
    private readonly Client _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientTests"/> class.
    /// </summary>
    public ClientTests()
    {
        string pipeName = $"test-pipe-{Guid.NewGuid()}";
        _mockRepository = new MockRepository(MockBehavior.Loose);
        _pipeClient = new NamedPipeClientStream(pipeName);
        _pipeServer = new NamedPipeServerStream(pipeName);
        _messageWriterMock = _mockRepository.Create<IMessageWriter>();
        _messageReaderMock = _mockRepository.Create<IMessageReader>();
        _client = new Client(_pipeClient, _messageWriterMock.Object, _messageReaderMock.Object);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
        _pipeClient.Dispose();
        _pipeServer.Dispose();
    }

    /// <summary>
    /// Tests that <see cref="Client.SendMessageAsync"/> sends the specified message using the message writer.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SendMessageAsync_should_SendSpecifiedMessage()
    {
        // Arrange
        Message cmd = PauseSessionCommand.CreateNew();
        await _pipeClient.ConnectAsync(CancellationToken);
        await _pipeServer.WaitForConnectionAsync();

        // Act
        await _client.SendMessageAsync(cmd, CancellationToken);

        // Assert
        _messageWriterMock.Verify(m => m.WriteAsync(_pipeClient, cmd, CancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="Client.ReceiveMessageAsync{T}"/> returns the expected message.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ReceiveMessageAsync_should_ReturnExpectedMessage()
    {
        // Arrange
        var cmd = PauseSessionCommand.CreateNew();
        await _pipeClient.ConnectAsync(CancellationToken);
        await _pipeServer.WaitForConnectionAsync();
        _messageReaderMock.Setup(m => m.ReadAsync<Message>(_pipeClient, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cmd);

        // Act
        var actual = await _client.ReceiveMessageAsync<PauseSessionCommand>(false, CancellationToken);

        // Assert
        Assert.Equal(cmd, actual);
    }

    /// <summary>
    /// Tests that <see cref="Client.ReceiveMessageAsync{T}"/> returns a buffered message
    /// without calling the message reader again if the message is already buffered.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ReceiveMessageAsync_should_ReturnBufferedMessage()
    {
        // Arrange
        var cancelCmd = CancelSessionCommand.CreateNew();
        var pauseCmd = PauseSessionCommand.CreateNew();
        await _pipeClient.ConnectAsync(CancellationToken);
        await _pipeServer.WaitForConnectionAsync();
        _messageReaderMock.SetupSequence(m => m.ReadAsync<Message>(_pipeClient, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cancelCmd)
            .ReturnsAsync(pauseCmd);
        await _client.ReceiveMessageAsync<PauseSessionCommand>(false, CancellationToken);
        _messageReaderMock.Reset();

        // Act
        var actual = await _client.ReceiveMessageAsync<CancelSessionCommand>(false, CancellationToken);

        // Assert
        Assert.Equal(cancelCmd, actual);
        _messageReaderMock.Verify(m => m.ReadAsync<Message>(_pipeClient, It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="Client.ReceiveMessageAsync{T}"/> throws an <see cref="InvalidOperationException"/>
    /// when the pipe is not connected.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ReceiveMessageAsync_should_ThrowInvalidOperationException_when_PipeIsNotConnected()
    {
        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _client.ReceiveMessageAsync<PauseSessionCommand>(false, CancellationToken));
    }

    /// <summary>
    /// Tests that <see cref="Client.DisposeAsync"/> sends a <see cref="ConnectionTerminationMessage"/> using the message writer.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisposeAsync_should_SendConnectionTerminationMessage()
    {
        // Arrange
        await _pipeClient.ConnectAsync(CancellationToken);
        await _pipeServer.WaitForConnectionAsync();

        // Act
        await _client.DisposeAsync();

        // Assert
        _messageWriterMock.Verify(
            m => m.WriteAsync<Message>(_pipeClient, It.IsAny<ConnectionTerminationMessage>(), CancellationToken),
            Times.Once);
    }
}