using Moq;

using Tiempito.Daemon.Application.Commands.Sessions;
using Tiempito.Daemon.Application.Sessions;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.Daemon.Tests.Application.Commands.Sessions;

/// <summary>
/// Unit tests for the <see cref="CancelSessionCommandHandler"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "Command")]
public sealed class CancelSessionCommandHandlerTests
{
    private readonly Mock<ISessionService> _sessionServiceMock;
    private readonly CancelSessionCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelSessionCommandHandlerTests"/> class.
    /// </summary>
    public CancelSessionCommandHandlerTests()
    {
        _sessionServiceMock = new Mock<ISessionService>();
        _handler = new CancelSessionCommandHandler(_sessionServiceMock.Object);
    }

    /// <summary>
    /// Tests that <see cref="CancelSessionCommandHandler.CanHandle"/> returns true when given a <see cref="CancelSessionCommand"/>.
    /// </summary>
    [Fact]
    public void CanHandle_should_ReturnTrue_when_CommandIsCancelSession()
    {
        // Arrange
        var command = CancelSessionCommand.CreateNew(sessionId: "session1");

        // Act
        bool actual = _handler.CanHandle(command);

        // Assert
        Assert.True(actual);
    }

    /// <summary>
    /// Tests that <see cref="CancelSessionCommandHandler.CanHandle"/> returns false when given a command that is not a <see cref="CancelSessionCommand"/>.
    /// </summary>
    [Fact]
    public void CanHandle_should_ReturnFalse_when_CommandIsNotCancelSession()
    {
        // Arrange
        var command = PauseSessionCommand.CreateNew(sessionId: "session1");

        // Act
        bool actual = _handler.CanHandle(command);

        // Assert
        Assert.False(actual);
    }

    /// <summary>
    /// Tests that <see cref="CancelSessionCommandHandler.HandleAsync"/> returns the expected <see cref="OperationResult"/>
    /// based on the success parameter, verifying correct interaction with <see cref="ISessionService.CancelSession"/>.
    /// </summary>
    /// <param name="success">Indicates whether the session cancel should succeed or fail.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HandleAsync_should_ReturnExpectedResult(bool success)
    {
        // Arrange
        var command = CancelSessionCommand.CreateNew(sessionId: "session1");
        var expectedResult = new OperationResult(success, Message: "Test message.");
        _sessionServiceMock.Setup(m => m.CancelSession(command.SessionId)).Returns(expectedResult);

        // Act
        OperationResult actualResult = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(expectedResult, actualResult);
        _sessionServiceMock.Verify(m => m.CancelSession(command.SessionId), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="CancelSessionCommandHandler.HandleAsync"/> throws an <see cref="ArgumentException"/>
    /// when the provided command is not a <see cref="CancelSessionCommand"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_should_ThrowArgumentException_when_CommandIsNotCancelSession()
    {
        // Arrange
        var command = PauseSessionCommand.CreateNew(sessionId: "session1");

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.HandleAsync(command).AsTask());
    }
}