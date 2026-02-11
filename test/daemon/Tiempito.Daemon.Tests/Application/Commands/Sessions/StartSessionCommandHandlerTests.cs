using Moq;

using Tiempito.Daemon.Application.Commands.Sessions;
using Tiempito.Daemon.Application.Sessions;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.Daemon.Tests.Application.Commands.Sessions;

/// <summary>
/// Unit tests for the <see cref="StartSessionCommandHandler"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "Command")]
public sealed class StartSessionCommandHandlerTests
{
    private readonly Mock<ISessionService> _sessionServiceMock;
    private readonly StartSessionCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartSessionCommandHandlerTests"/> class.
    /// </summary>
    public StartSessionCommandHandlerTests()
    {
        _sessionServiceMock = new Mock<ISessionService>();
        _handler = new StartSessionCommandHandler(_sessionServiceMock.Object);
    }

    /// <summary>
    /// Tests that <see cref="StartSessionCommandHandler.CanHandle"/> returns true when given a <see cref="StartSessionCommand"/>.
    /// </summary>
    [Fact]
    public void CanHandle_should_ReturnTrue_when_CommandIsStartSession()
    {
        // Arrange
        var command = StartSessionCommand.CreateNew(sessionId: "session1", sessionConfigId: "config1");

        // Act
        bool actual = _handler.CanHandle(command);

        // Assert
        Assert.True(actual);
    }

    /// <summary>
    /// Tests that <see cref="StartSessionCommandHandler.CanHandle"/> returns false when given a command that is not a <see cref="StartSessionCommand"/>.
    /// </summary>
    [Fact]
    public void CanHandle_should_ReturnFalse_when_CommandIsNotStartSession()
    {
        // Arrange
        var command = PauseSessionCommand.CreateNew(sessionId: "session1");

        // Act
        bool actual = _handler.CanHandle(command);

        // Assert
        Assert.False(actual);
    }

    /// <summary>
    /// Tests that <see cref="StartSessionCommandHandler.HandleAsync"/> returns the expected <see cref="OperationResult"/>
    /// based on the success parameter, verifying correct interaction with <see cref="ISessionService.StartSessionAsync"/>.
    /// </summary>
    /// <param name="success">Indicates whether the session start should succeed or fail.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HandleAsync_should_ReturnExpectedResult(bool success)
    {
        // Arrange
        var command = StartSessionCommand.CreateNew(sessionId: "session1", sessionConfigId: "config1");
        var expectedResult = new OperationResult(success, Message: "Test message.");
        _sessionServiceMock.Setup(m => m.StartSessionAsync(command.SessionId, command.SessionConfigId))
            .ReturnsAsync(expectedResult);

        // Act
        OperationResult actualResult = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(expectedResult, actualResult);
        _sessionServiceMock.Verify(m => m.StartSessionAsync(command.SessionId, command.SessionConfigId), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="StartSessionCommandHandler.HandleAsync"/> throws an <see cref="ArgumentException"/>
    /// when the provided command is not a <see cref="StartSessionCommand"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_should_ThrowArgumentException_when_CommandIsNotStartSession()
    {
        // Arrange
        var command = PauseSessionCommand.CreateNew(sessionId: "session1");

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.HandleAsync(command).AsTask());
    }
}