using Moq;

using Tiempito.Daemon.Application.Commands.Sessions;
using Tiempito.Daemon.Application.Sessions;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.Daemon.Tests.Application.Commands.Sessions;

/// <summary>
/// Unit tests for the <see cref="ResumeSessionCommandHandler"/> class.
/// </summary>
[Trait("Commands", "Unit")]
public sealed class ResumeSessionCommandHandlerTests
{
    private readonly Mock<ISessionService> _sessionServiceMock;
    private readonly ResumeSessionCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResumeSessionCommandHandlerTests"/> class.
    /// </summary>
    public ResumeSessionCommandHandlerTests()
    {
        _sessionServiceMock = new Mock<ISessionService>();
        _handler = new ResumeSessionCommandHandler(_sessionServiceMock.Object);
    }

    /// <summary>
    /// Tests that <see cref="ResumeSessionCommandHandler.CanHandle"/> returns true when given a <see cref="ResumeSessionCommand"/>.
    /// </summary>
    [Fact]
    public void CanHandle_should_ReturnTrue_when_CommandIsResumeSession()
    {
        // Arrange
        var command = ResumeSessionCommand.CreateNew(sessionId: "session1");

        // Act
        bool actual = _handler.CanHandle(command);

        // Assert
        Assert.True(actual);
    }

    /// <summary>
    /// Tests that <see cref="ResumeSessionCommandHandler.CanHandle"/> returns false when given a command that is not a <see cref="ResumeSessionCommand"/>.
    /// </summary>
    [Fact]
    public void CanHandle_should_ReturnFalse_when_CommandIsNotResumeSession()
    {
        // Arrange
        var command = PauseSessionCommand.CreateNew(sessionId: "session1");

        // Act
        bool actual = _handler.CanHandle(command);

        // Assert
        Assert.False(actual);
    }

    /// <summary>
    /// Tests that <see cref="ResumeSessionCommandHandler.HandleAsync"/> returns the expected <see cref="OperationResult"/>
    /// based on the success parameter, verifying correct interaction with <see cref="ISessionService.ResumeSession"/>.
    /// </summary>
    /// <param name="success">Indicates whether the session resume should succeed or fail.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HandleAsync_should_ReturnExpectedResult(bool success)
    {
        // Arrange
        var command = ResumeSessionCommand.CreateNew(sessionId: "session1");
        var expectedResult = new OperationResult(success, Message: "Test message.");
        _sessionServiceMock.Setup(m => m.ResumeSession(command.SessionId)).Returns(expectedResult);

        // Act
        OperationResult actualResult = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(expectedResult, actualResult);
        _sessionServiceMock.Verify(m => m.ResumeSession(command.SessionId), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="ResumeSessionCommandHandler.HandleAsync"/> throws an <see cref="ArgumentException"/>
    /// when the provided command is not a <see cref="ResumeSessionCommand"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_should_ThrowArgumentException_when_CommandIsNotResumeSession()
    {
        // Arrange
        var command = PauseSessionCommand.CreateNew(sessionId: "session1");

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.HandleAsync(command).AsTask());
    }
}