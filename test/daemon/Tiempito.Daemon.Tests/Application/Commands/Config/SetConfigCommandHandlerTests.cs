using Moq;

using Tiempito.Daemon.Application.Commands.Config;
using Tiempito.Daemon.Application.Config.User;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models.Commands.Config;
using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.Daemon.Tests.Application.Commands.Config;

/// <summary>
/// Unit tests for the <see cref="SetConfigCommandHandler"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "Command")]
public sealed class SetConfigCommandHandlerTests
{
    private readonly Mock<IUserConfigService> _userConfigServiceMock;
    private readonly SetConfigCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetConfigCommandHandlerTests"/> class.
    /// </summary>
    public SetConfigCommandHandlerTests()
    {
        _userConfigServiceMock = new Mock<IUserConfigService>();
        _handler = new SetConfigCommandHandler(_userConfigServiceMock.Object);
    }

    /// <summary>
    /// Tests that <see cref="SetConfigCommandHandler.CanHandle"/> returns true when the command is <see cref="SetConfigCommand"/>.
    /// </summary>
    [Fact]
    public void CanHandle_should_ReturnTrue_when_CommandIsSetConfig()
    {
        // Arrange
        var command = SetConfigCommand.CreateNew(defaultSessionConfigId: "config1");

        // Act
        bool actual = _handler.CanHandle(command);

        // Assert
        Assert.True(actual);
    }

    /// <summary>
    /// Tests that <see cref="SetConfigCommandHandler.CanHandle"/> returns false when the command is not <see cref="SetConfigCommand"/>.
    /// </summary>
    [Fact]
    public void CanHandle_should_ReturnFalse_when_CommandIsNotSetConfig()
    {
        // Arrange
        var command = StartSessionCommand.CreateNew();

        // Act
        bool actual = _handler.CanHandle(command);

        // Assert
        Assert.False(actual);
    }

    /// <summary>
    /// Tests that <see cref="SetConfigCommandHandler.HandleAsync"/> returns the expected <see cref="OperationResult"/>
    /// based on the result of <see cref="IUserConfigService.ChangeDefaultSessionConfigAsync"/>.
    /// </summary>
    /// <param name="success">Indicates whether the operation should succeed or fail.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HandleAsync_should_ReturnExpectedResult(bool success)
    {
        // Arrange
        var command = SetConfigCommand.CreateNew(defaultSessionConfigId: "config1");
        var expectedResult = new OperationResult(success, Message: "Test message.");
        _userConfigServiceMock.Setup(m => m.ChangeDefaultSessionConfigAsync(command.DefaultSessionConfigId))
            .ReturnsAsync(expectedResult);

        // Act
        OperationResult actualResult = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(expectedResult, actualResult);
        _userConfigServiceMock.Verify(
            m => m.ChangeDefaultSessionConfigAsync(command.DefaultSessionConfigId), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="SetConfigCommandHandler.HandleAsync"/> throws an <see cref="ArgumentException"/>
    /// when the command is not a <see cref="SetConfigCommand"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_should_ThrowArgumentException_when_CommandIsNotSetConfig()
    {
        // Arrange
        var command = StartSessionCommand.CreateNew();

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.HandleAsync(command).AsTask());
    }
}