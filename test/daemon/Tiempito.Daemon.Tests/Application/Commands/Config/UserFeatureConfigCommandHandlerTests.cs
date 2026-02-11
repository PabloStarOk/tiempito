using Moq;

using Tiempito.Daemon.Application.Commands.Config;
using Tiempito.Daemon.Application.Config.User;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models.Commands.Config;
using Tiempito.IPC.Models.Commands.Session;
using Tiempito.IPC.Models.Enums;

namespace Tiempito.Daemon.Tests.Application.Commands.Config;

/// <summary>
/// Unit tests for the <see cref="UserFeatureConfigCommandHandler"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "Command")]
public sealed class UserFeatureConfigCommandHandlerTests
{
    private readonly Mock<IUserConfigService> _userConfigServiceMock;
    private readonly UserFeatureConfigCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserFeatureConfigCommandHandlerTests"/> class.
    /// </summary>
    public UserFeatureConfigCommandHandlerTests()
    {
        _userConfigServiceMock = new Mock<IUserConfigService>();
        _handler = new UserFeatureConfigCommandHandler(_userConfigServiceMock.Object);
    }

    /// <summary>
    /// Tests that <see cref="UserFeatureConfigCommandHandler.CanHandle"/> returns true when the command is a <see cref="UserFeatureConfigCommand"/>.
    /// </summary>
    [Fact]
    public void CanHandle_should_ReturnTrue_when_CommandIsUserFeatureConfigCommand()
    {
        // Arrange
        var command = UserFeatureConfigCommand.CreateNew(enable: true, UserFeature.Notification);

        // Act
        bool actual = _handler.CanHandle(command);

        // Assert
        Assert.True(actual);
    }

    /// <summary>
    /// Tests that <see cref="UserFeatureConfigCommandHandler.CanHandle"/> returns false when the command is not a <see cref="UserFeatureConfigCommand"/>.
    /// </summary>
    [Fact]
    public void CanHandle_should_ReturnFalse_when_CommandIsNotUserFeatureConfigCommand()
    {
        // Arrange
        var command = StartSessionCommand.CreateNew();

        // Act
        bool actual = _handler.CanHandle(command);

        // Assert
        Assert.False(actual);
    }

    /// <summary>
    /// Tests that <see cref="UserFeatureConfigCommandHandler.HandleAsync"/> returns the expected result
    /// and calls <see cref="IUserConfigService.EnableFeatureAsync"/> when the command enables a feature.
    /// </summary>
    /// <param name="success">Indicates whether the operation should succeed.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HandleAsync_should_ReturnExpectedResultAndCallEnableFeatureAsync(bool success)
    {
        // Arrange
        var command = UserFeatureConfigCommand.CreateNew(enable: true, UserFeature.Notification);
        var expectedResult = new OperationResult(success, "Test message");
        _userConfigServiceMock.Setup(m => m.EnableFeatureAsync(command.Feature)).ReturnsAsync(expectedResult);

        // Act
        var actualResult = await _handler.HandleAsync(command).AsTask();

        // Assert
        Assert.Equal(expectedResult, actualResult);
        _userConfigServiceMock.Verify(m => m.EnableFeatureAsync(command.Feature), Times.Once);
        _userConfigServiceMock.Verify(m => m.DisableFeatureAsync(command.Feature), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="UserFeatureConfigCommandHandler.HandleAsync"/> returns the expected result
    /// and calls <see cref="IUserConfigService.DisableFeatureAsync"/> when the command disables a feature.
    /// </summary>
    /// <param name="success">Indicates whether the operation should succeed.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HandleAsync_should_ReturnExpectedResultAndCallDisableFeatureAsync(bool success)
    {
        // Arrange
        var command = UserFeatureConfigCommand.CreateNew(enable: false, UserFeature.Notification);
        var expectedResult = new OperationResult(success, "Test message");
        _userConfigServiceMock.Setup(m => m.DisableFeatureAsync(command.Feature)).ReturnsAsync(expectedResult);

        // Act
        var actualResult = await _handler.HandleAsync(command).AsTask();

        // Assert
        Assert.Equal(expectedResult, actualResult);
        _userConfigServiceMock.Verify(m => m.DisableFeatureAsync(command.Feature), Times.Once);
        _userConfigServiceMock.Verify(m => m.EnableFeatureAsync(command.Feature), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="UserFeatureConfigCommandHandler.HandleAsync"/> throws an <see cref="ArgumentException"/>
    /// when the command is not a <see cref="UserFeatureConfigCommand"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_should_ThrowArgumentException_when_CommandIsNotUserFeatureConfigCommand()
    {
        // Arrange
        var command = StartSessionCommand.CreateNew();

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.HandleAsync(command).AsTask());
    }
}