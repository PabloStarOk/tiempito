using Moq;

using Tiempito.CLI.Exceptions;
using Tiempito.CLI.Services.Abstractions;
using Tiempito.CLI.Services.Implementations;
using Tiempito.IPC.Models;
using Tiempito.IPC.Models.Commands.Session;
using Tiempito.IPC.Models.Enums;

namespace Tiempito.CLI.UnitTests.Services.Implementations;

/// <summary>
/// Unit tests for the <see cref="CommandSender"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature ", "CLI")]
public sealed class CommandSenderTests : IDisposable
{
    private static readonly CancellationToken CancellationToken = CancellationToken.None;
    private readonly MockRepository _mockRepository;
    private readonly Mock<IClient> _clientMock;
    private readonly CommandSender _commandSender;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandSenderTests"/> class.
    /// </summary>
    public CommandSenderTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Loose);
        _clientMock = _mockRepository.Create<IClient>();
        _commandSender = new CommandSender(_clientMock.Object);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Tests that <see cref="CommandSender.SendAsync"/> returns the expected response
    /// when called with a valid command.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SendAsync_should_ReturnExpectedResponse()
    {
        // Arrange
        var cmd = PauseSessionCommand.CreateNew();
        var response = Response.Ok(cmd.CorrelationId, "Test");
        _clientMock.Setup(m => m.SendMessageAsync(cmd, CancellationToken));
        _clientMock.Setup(m => m.ReceiveMessageAsync<Response>(useTimeout: true, CancellationToken))
            .ReturnsAsync(response);

        // Act
        var actual = await _commandSender.SendAsync(cmd);

        // Assert
        Assert.Equal(response, actual);
    }

    /// <summary>
    /// Tests that <see cref="CommandSender.SendAsync"/> returns an error response
    /// when the client throws a <see cref="ResponseTimeoutException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SendAsync_should_ReturnErrorResponse_when_ClientThrowsResponseTimeoutException()
    {
        // Arrange
        var cmd = PauseSessionCommand.CreateNew();
        _clientMock.Setup(m => m.SendMessageAsync(cmd, CancellationToken));
        _clientMock.Setup(m => m.ReceiveMessageAsync<Response>(useTimeout: true, CancellationToken))
            .ThrowsAsync(new ResponseTimeoutException());

        // Act
        var actual = await _commandSender.SendAsync(cmd);

        // Assert
        Assert.False(actual.Success);
        Assert.Equal(cmd.CorrelationId, actual.CorrelationId);
        Assert.Equal(ResponseStatusCode.Error, actual.StatusCode);
    }

    /// <summary>
    /// Tests that <see cref="CommandSender.SendAsync"/> returns an error response
    /// when the client throws a <see cref="TimeoutException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SendAsync_should_ReturnErrorResponse_when_ClientThrowsTimeoutException()
    {
        // Arrange
        var cmd = PauseSessionCommand.CreateNew();
        _clientMock.Setup(m => m.SendMessageAsync(cmd, CancellationToken));
        _clientMock.Setup(m => m.ReceiveMessageAsync<Response>(useTimeout: true, CancellationToken))
            .ThrowsAsync(new TimeoutException());

        // Act
        var actual = await _commandSender.SendAsync(cmd);

        // Assert
        Assert.False(actual.Success);
        Assert.Equal(cmd.CorrelationId, actual.CorrelationId);
        Assert.Equal(ResponseStatusCode.Error, actual.StatusCode);
    }
}