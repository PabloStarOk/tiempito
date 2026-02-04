using Moq;

using Tiempito.Daemon.Application.Commands.Sessions;
using Tiempito.Daemon.Application.Sessions;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.Daemon.Tests.Application.Commands.Sessions;

/// <summary>
/// Unit tests for the <see cref="PauseSessionCommandHandler"/> class.
/// </summary>
[Trait("Commands", "Unit")]
public sealed class PauseSessionCommandHandlerTests
{
    private readonly Mock<ISessionService> _sessionServiceMock;
    private readonly PauseSessionCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="PauseSessionCommandHandlerTests"/> class.
    /// </summary>
    public PauseSessionCommandHandlerTests()
    {
        _sessionServiceMock = new Mock<ISessionService>();
        _handler = new PauseSessionCommandHandler(_sessionServiceMock.Object);
    }

    /// <summary>
    /// Tests that <see cref="PauseSessionCommandHandler.CanHandle"/> returns true when given a <see cref="PauseSessionCommand"/>.
    /// </summary>
    [Fact]
    public void CanHandle_should_ReturnTrue_when_CommandIsPauseSession()
    {
        // Arrange
        var command = PauseSessionCommand.CreateNew(sessionId: "session1");

        // Act
        bool actual = _handler.CanHandle(command);

        // Assert
        Assert.True(actual);
    }

    /// <summary>
    /// Tests that <see cref="PauseSessionCommandHandler.CanHandle"/> returns false when given a command that is not a <see cref="PauseSessionCommand"/>.
    /// </summary>
    [Fact]
    public void CanHandle_should_ReturnFalse_when_CommandIsNotPauseSession()
    {
        // Arrange
        var command = StartSessionCommand.CreateNew(sessionId: "session1");

        // Act
        bool actual = _handler.CanHandle(command);

        // Assert
        Assert.False(actual);
    }

    /// <summary>
    /// Tests that <see cref="PauseSessionCommandHandler.HandleAsync"/> returns the expected <see cref="OperationResult"/>
    /// based on the success parameter, verifying correct interaction with <see cref="ISessionService.PauseSession"/>.
    /// </summary>
    /// <param name="success">Indicates whether the session pause should succeed or fail.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HandleAsync_should_ReturnExpectedResult(bool success)
    {
        // Arrange
        var command = PauseSessionCommand.CreateNew(sessionId: "session1");
        var expectedResult = new OperationResult(success, Message: "Test message.");
        _sessionServiceMock.Setup(m => m.PauseSession(command.SessionId)).Returns(expectedResult);

        // Act
        OperationResult actualResult = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(expectedResult, actualResult);
        _sessionServiceMock.Verify(m => m.PauseSession(command.SessionId), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="PauseSessionCommandHandler.HandleAsync"/> throws an <see cref="ArgumentException"/>
    /// when the provided command is not a <see cref="PauseSessionCommand"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_should_ThrowArgumentException_when_CommandIsNotPauseSession()
    {
        // Arrange
        var command = StartSessionCommand.CreateNew(sessionId: "session1");

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.HandleAsync(command).AsTask());
    }
}