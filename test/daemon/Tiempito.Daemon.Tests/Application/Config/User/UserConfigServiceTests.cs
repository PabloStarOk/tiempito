using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Tiempito.Daemon.Application.Config.User;
using Tiempito.Daemon.Application.Shared;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models.Enums;

namespace Tiempito.Daemon.Tests.Application.Config.User;

/// <summary>
/// Unit tests for the <see cref="UserConfigService"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "Config")]
public sealed class UserConfigServiceTests : IDisposable
{
    private readonly MockRepository _mockRepository;
    private readonly Mock<IUserConfigWriter> _userConfigWriterMock;
    private readonly Mock<IOptionsMonitor<UserConfig>> _userConfigOptionsMock;
    private readonly Mock<IFileSystem> _fileSystemMock;
    private readonly UserConfigService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConfigServiceTests"/> class.
    /// </summary>
    public UserConfigServiceTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Loose);
        var loggerMock = _mockRepository.Create<ILogger<UserConfigService>>();
        _userConfigWriterMock = _mockRepository.Create<IUserConfigWriter>();
        _userConfigOptionsMock = _mockRepository.Create<IOptionsMonitor<UserConfig>>();
        _fileSystemMock = _mockRepository.Create<IFileSystem>();
        _service = new UserConfigService(
            loggerMock.Object,
            _userConfigOptionsMock.Object,
            _userConfigWriterMock.Object,
            _fileSystemMock.Object);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.StartAsync"/> creates the user config file when it does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StartAsync_should_CreateUserConfigFile_when_ItDoesNotExist()
    {
        // Arrange
        _fileSystemMock.Setup(m => m.Path.Exists(Paths.UserConfigFilePath)).Returns(false);
        _fileSystemMock.Setup(m => m.File.Create(Paths.UserConfigFilePath)).Returns(MockFileStream.Null);

        // Act
        await _service.StartAsync();

        // Assert
        _fileSystemMock.Verify(m => m.Path.Exists(Paths.UserConfigFilePath), Times.Once);
        _fileSystemMock.Verify(m => m.File.Create(Paths.UserConfigFilePath), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.StartAsync"/> does not create the user config file when it already exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StartAsync_should_NotCreateUserConfigFile_when_ItAlreadyExists()
    {
        // Arrange
        _fileSystemMock.Setup(m => m.Path.Exists(Paths.UserConfigFilePath)).Returns(true);

        // Act
        await _service.StartAsync();

        // Assert
        _fileSystemMock.Verify(m => m.Path.Exists(Paths.UserConfigFilePath), Times.Once);
        _fileSystemMock.Verify(m => m.File.Create(Paths.UserConfigFilePath), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.ChangeDefaultSessionConfigAsync"/> returns success
    /// when a valid new default session config ID is provided and the save operation succeeds.
    /// </summary>
    /// <param name="currentId">The current default session config ID.</param>
    /// <param name="expectedId">The new default session config ID to set.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(null, "1")]
    [InlineData("1", null)]
    public async Task ChangeDefaultSessionConfigAsync_should_ReturnSuccess_when_IdIsValidAndSaveSucceeds(
        string? currentId,
        string? expectedId)
    {
        // Arrange
        var userConfig = new UserConfig(currentId, []);
        var expectedUserConfig = userConfig with { DefaultConfigId = expectedId };
        await SetupMocksAsync(userConfig, configFileExists: true, successfulSave: true);

        // Act
        OperationResult actual = await _service.ChangeDefaultSessionConfigAsync(expectedId);

        // Assert
        Assert.True(actual.Success);
        _userConfigWriterMock.Verify(m => m.Write(expectedUserConfig), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.ChangeDefaultSessionConfigAsync"/> returns an error
    /// when the provided ID is the same as the current default session config ID (case-insensitive).
    /// </summary>
    /// <param name="defaultInitialId">The current default session config ID.</param>
    /// <param name="inputId">The input ID to set as the new default session config ID.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("test", "TEST")]
    [InlineData("TEST", "test")]
    public async Task ChangeDefaultSessionConfigAsync_should_ReturnError_when_IdIsSameAsCurrent(
        string defaultInitialId,
        string inputId)
    {
        // Arrange
        var userConfig = new UserConfig(defaultInitialId, []);
        _userConfigOptionsMock.Setup(m => m.CurrentValue).Returns(userConfig);

        // Act
        OperationResult actual = await _service.ChangeDefaultSessionConfigAsync(inputId);

        // Assert
        Assert.False(actual.Success);
        _userConfigWriterMock.Verify(m => m.Write(It.IsAny<UserConfig>()), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.ChangeDefaultSessionConfigAsync"/> returns an error
    /// when the save operation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ChangeDefaultSessionConfigAsync_should_ReturnError_when_SaveOperationFails()
    {
        // Arrange
        const string defaultInitialId = "1";
        var userConfig = new UserConfig(null, []);
        await SetupMocksAsync(userConfig, configFileExists: true, successfulSave: false);

        // Act
        OperationResult actual = await _service.ChangeDefaultSessionConfigAsync(defaultInitialId);

        // Assert
        Assert.False(actual.Success);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.EnableFeatureAsync"/> returns success
    /// when the feature is disabled and the save operation succeeds.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EnableFeatureAsync_should_ReturnSuccess_when_ItIsDisabledAndSaveSucceeds()
    {
        // Arrange
        const UserFeature feature = UserFeature.Notification;
        var userConfig = new UserConfig(null, []);
        await SetupMocksAsync(userConfig, configFileExists: true, successfulSave: true);

        // Act
        OperationResult actual = await _service.EnableFeatureAsync(feature);

        // Assert
        Assert.True(actual.Success);
        _userConfigWriterMock.Verify(
            m => m.Write(It.Is<UserConfig>(u => u.EnabledFeatures.Contains(feature))),
            Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.EnableFeatureAsync"/> returns an error
    /// when the feature is already enabled.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EnableFeatureAsync_should_ReturnError_when_FeatureIsAlreadyEnabled()
    {
        // Arrange
        var userConfig = new UserConfig(null, [UserFeature.Notification]);
        _userConfigOptionsMock.Setup(m => m.CurrentValue).Returns(userConfig);

        // Act
        OperationResult actual = await _service.EnableFeatureAsync(UserFeature.Notification);

        // Assert
        Assert.False(actual.Success);
        _userConfigWriterMock.Verify(m => m.Write(It.IsAny<UserConfig>()), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.EnableFeatureAsync"/> returns an error
    /// when the save operation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EnableFeatureAsync_should_ReturnError_when_SaveOperationFails()
    {
        // Arrange
        var userConfig = new UserConfig(null, []);
        await SetupMocksAsync(userConfig, configFileExists: true, successfulSave: false);

        // Act
        OperationResult actual = await _service.EnableFeatureAsync(UserFeature.Notification);

        // Assert
        Assert.False(actual.Success);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.DisableFeatureAsync"/> returns success
    /// when the feature is enabled and the save operation succeeds.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisableFeatureAsync_should_ReturnSuccess_when_ItIsEnabledAndSaveSucceeds()
    {
        // Arrange
        const UserFeature feature = UserFeature.Notification;
        var userConfig = new UserConfig(null, [feature]);
        await SetupMocksAsync(userConfig, configFileExists: true, successfulSave: true);

        // Act
        OperationResult actual = await _service.DisableFeatureAsync(feature);

        // Assert
        Assert.True(actual.Success);
        _userConfigWriterMock.Verify(
            m => m.Write(It.Is<UserConfig>(u => !u.EnabledFeatures.Contains(feature))),
            Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.DisableFeatureAsync"/> returns an error
    /// when the feature is already disabled.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisableFeatureAsync_should_ReturnError_when_FeatureIsAlreadyDisabled()
    {
        // Arrange
        var userConfig = new UserConfig(null, []);
        _userConfigOptionsMock.Setup(m => m.CurrentValue).Returns(userConfig);

        // Act
        OperationResult actual = await _service.DisableFeatureAsync(UserFeature.Notification);

        // Assert
        Assert.False(actual.Success);
        _userConfigWriterMock.Verify(m => m.Write(It.IsAny<UserConfig>()), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.DisableFeatureAsync"/> returns an error
    /// when the save operation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisableFeatureAsync_should_ReturnError_when_SaveOperationFails()
    {
        // Arrange
        var userConfig = new UserConfig(null, [UserFeature.Notification]);
        await SetupMocksAsync(userConfig, configFileExists: true, successfulSave: false);

        // Act
        OperationResult actual = await _service.DisableFeatureAsync(UserFeature.Notification);

        // Assert
        Assert.False(actual.Success);
    }

    private async Task SetupMocksAsync(UserConfig userConfig, bool configFileExists, bool successfulSave)
    {
        _fileSystemMock.Setup(m => m.Path.Exists(Paths.UserConfigFilePath)).Returns(configFileExists);
        _userConfigWriterMock.Setup(m => m.Write(It.IsAny<UserConfig>())).Returns(successfulSave);
        _userConfigOptionsMock.Setup(m => m.CurrentValue).Returns(userConfig);
        await _service.StartAsync();
    }
}