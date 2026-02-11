using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

using Moq;

using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Config.User;
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
    private readonly Mock<IUserConfigReader> _userConfigReaderMock;
    private readonly Mock<IFileProvider> _fileProviderMock;
    private readonly Mock<IFileInfo> _fileInfoMock;
    private readonly UserConfigService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConfigServiceTests"/> class.
    /// </summary>
    public UserConfigServiceTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Loose);
        var loggerMock = _mockRepository.Create<ILogger<UserConfigService>>();
        _userConfigWriterMock = _mockRepository.Create<IUserConfigWriter>();
        _userConfigReaderMock = _mockRepository.Create<IUserConfigReader>();
        _fileProviderMock = _mockRepository.Create<IFileProvider>();
        _fileInfoMock = _mockRepository.Create<IFileInfo>();
        _service = new UserConfigService(
            loggerMock.Object,
            _userConfigReaderMock.Object,
            _userConfigWriterMock.Object,
            _fileProviderMock.Object);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.StartAsync"/> reads the user config
    /// when the config file exists or does not exist.
    /// </summary>
    /// <param name="configFileExists">Indicates if the config file exists.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task StartAsync_should_ReadUserConfig(bool configFileExists)
    {
        // Arrange
        var userConfig = new UserConfig();
        _fileInfoMock.Setup(f => f.Exists).Returns(configFileExists);
        _fileProviderMock.Setup(m => m.GetFileInfo(AppConfigConstants.UserConfigFileName))
            .Returns(_fileInfoMock.Object);
        _userConfigReaderMock.Setup(r => r.Read()).Returns(userConfig);

        // Act
        await _service.StartAsync();

        // Assert
        Assert.Equal(userConfig, _service.UserConfig);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.ChangeDefaultSessionConfigAsync"/> returns a successful <see cref="OperationResult"/>
    /// and updates the <see cref="UserConfig.DefaultSessionId"/> when the ID is valid and save succeeds.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ChangeDefaultSessionConfigAsync_should_UpdateDefaultSessionId_when_IdIsValidAndSaveSucceeds()
    {
        // Arrange
        const string expectedId = "1";
        var userConfig = new UserConfig(defaultSessionId: string.Empty);
        await SetupMocksAsync(userConfig, configFileExists: true, successfulSave: true);

        // Act
        OperationResult actual = await _service.ChangeDefaultSessionConfigAsync(expectedId);

        // Assert
        Assert.True(actual.Success);
        Assert.Equal(expectedId, userConfig.DefaultSessionId);
        _userConfigWriterMock.Verify(m => m.Write(userConfig), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.ChangeDefaultSessionConfigAsync"/> allows setting the default session ID to null,
    /// effectively resetting the default session.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ChangeDefaultSessionConfigAsync_should_ResetDefaultSessionId_when_IdIsNull()
    {
        // Arrange
        const string? expectedId = null;
        var userConfig = new UserConfig(defaultSessionId: string.Empty);
        await SetupMocksAsync(userConfig, configFileExists: true, successfulSave: true);

        // Act
        OperationResult actual = await _service.ChangeDefaultSessionConfigAsync(expectedId);

        // Assert
        Assert.True(actual.Success);
        Assert.Equal(expectedId, userConfig.DefaultSessionId);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.ChangeDefaultSessionConfigAsync"/> returns an error <see cref="OperationResult"/>
    /// when the save operation fails, and ensures the default session ID is rolled back and the event is not raised.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ChangeDefaultSessionConfigAsync_should_ReturnErrorAndRollback_when_SaveOperationFails()
    {
        // Arrange
        const string defaultInitialId = "1";
        var userConfig = new UserConfig(defaultInitialId);
        await SetupMocksAsync(userConfig, configFileExists: true, successfulSave: false);
        bool raised = false;
        _service.OnConfigChanged += (_, _) => { raised = true; };

        // Act
        OperationResult actual = await _service.ChangeDefaultSessionConfigAsync("2");

        // Assert
        Assert.False(actual.Success);
        Assert.Equal(defaultInitialId, userConfig.DefaultSessionId);
        Assert.False(raised);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.ChangeDefaultSessionConfigAsync"/> returns an error <see cref="OperationResult"/>
    /// when the provided session ID is the same as the current default session ID.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ChangeDefaultSessionConfigAsync_should_ReturnError_when_IdIsSameAsCurrent()
    {
        // Arrange
        const string defaultInitialId = "1";
        var userConfig = new UserConfig(defaultInitialId);
        bool raised = false;
        _service.OnConfigChanged += (_, _) => { raised = true; };

        // Act
        OperationResult actual = await _service.ChangeDefaultSessionConfigAsync(defaultInitialId);

        // Assert
        Assert.False(actual.Success);
        Assert.Equal(defaultInitialId, userConfig.DefaultSessionId);
        _userConfigWriterMock.Verify(m => m.Write(userConfig), Times.Never);
        Assert.False(raised);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.EnableFeatureAsync"/> enables the specified feature
    /// and updates the user configuration accordingly when the feature was disabled and save succeeds.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EnableFeatureAsync_should_EnableFeature_when_ItIsDisabledAndSaveSucceeds()
    {
        // Arrange
        var userConfig = new UserConfig();
        userConfig.DisableFeature(UserFeature.Notification);
        await SetupMocksAsync(userConfig, configFileExists: true, successfulSave: true);

        // Act
        OperationResult actual = await _service.EnableFeatureAsync(UserFeature.Notification);

        // Assert
        Assert.True(actual.Success);
        Assert.Single(userConfig.EnabledFeatures);
        Assert.Contains(UserFeature.Notification, userConfig.EnabledFeatures);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.EnableFeatureAsync"/> returns an error when
    /// the feature is already enabled and does not raise the <see cref="UserConfigService.OnConfigChanged"/> event.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EnableFeatureAsync_should_ReturnError_when_FeatureIsAlreadyEnabled()
    {
        // Arrange
        var userConfig = new UserConfig();
        userConfig.EnableFeature(UserFeature.Notification);
        bool raised = false;
        _service.OnConfigChanged += (_, _) => { raised = true; };

        // Act
        OperationResult actual = await _service.EnableFeatureAsync(UserFeature.Notification);

        // Assert
        Assert.False(actual.Success);
        Assert.False(raised);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.EnableFeatureAsync"/> returns an error <see cref="OperationResult"/>
    /// when the save operation fails, and ensures the feature changes are rolled back and the event is not raised.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task EnableFeatureAsync_should_ReturnErrorAndRollback_when_SaveOperationFails()
    {
        // Arrange
        var userConfig = new UserConfig();
        userConfig.DisableFeature(UserFeature.Notification);
        await SetupMocksAsync(userConfig, configFileExists: true, successfulSave: false);
        bool raised = false;
        _service.OnConfigChanged += (_, _) => { raised = true; };

        // Act
        OperationResult actual = await _service.EnableFeatureAsync(UserFeature.Notification);

        // Assert
        Assert.False(actual.Success);
        Assert.Empty(userConfig.EnabledFeatures);
        Assert.False(raised);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.DisableFeatureAsync"/> disables the specified feature
    /// and updates the user configuration accordingly when the feature was enabled and save succeeds.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisableFeatureAsync_should_DisableFeature_when_ItIsEnabledAndSaveSucceeds()
    {
        // Arrange
        var userConfig = new UserConfig();
        userConfig.EnableFeature(UserFeature.Notification);
        await SetupMocksAsync(userConfig, configFileExists: true, successfulSave: true);

        // Act
        OperationResult actual = await _service.DisableFeatureAsync(UserFeature.Notification);

        // Assert
        Assert.True(actual.Success);
        Assert.Empty(userConfig.EnabledFeatures);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.DisableFeatureAsync"/> returns an error when
    /// the feature is already disabled and does not raise the <see cref="UserConfigService.OnConfigChanged"/> event.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisableFeatureAsync_should_ReturnError_when_FeatureIsAlreadyDisabled()
    {
        // Arrange
        var userConfig = new UserConfig();
        userConfig.DisableFeature(UserFeature.Notification);
        bool raised = false;
        _service.OnConfigChanged += (_, _) => { raised = true; };

        // Act
        OperationResult actual = await _service.DisableFeatureAsync(UserFeature.Notification);

        // Assert
        Assert.False(actual.Success);
        Assert.False(raised);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigService.DisableFeatureAsync"/> returns an error <see cref="OperationResult"/>
    /// when the save operation fails, and ensures the feature changes are rolled back and the event is not raised.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisableFeatureAsync_should_ReturnErrorAndRollback_when_SaveOperationFails()
    {
        // Arrange
        var userConfig = new UserConfig();
        userConfig.EnableFeature(UserFeature.Notification);
        await SetupMocksAsync(userConfig, configFileExists: true, successfulSave: false);
        bool raised = false;
        _service.OnConfigChanged += (_, _) => { raised = true; };

        // Act
        OperationResult actual = await _service.DisableFeatureAsync(UserFeature.Notification);

        // Assert
        Assert.False(actual.Success);
        Assert.Single(userConfig.EnabledFeatures);
        Assert.Contains(UserFeature.Notification, userConfig.EnabledFeatures);
        Assert.False(raised);
    }

    /// <summary>
    /// Tests that the <see cref="UserConfigService.OnConfigChanged"/> event is raised
    /// when the default session configuration is changed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task OnConfigChanged_should_BeRaised_when_DefaultSessionConfigIsChanged()
    {
        // Arrange
        const string newId = "1";
        bool raised = false;
        _service.OnConfigChanged += (_, _) => { raised = true; };
        _userConfigWriterMock.Setup(m => m.Write(It.IsAny<UserConfig>())).Returns(true);

        // Act
        await _service.ChangeDefaultSessionConfigAsync(newId);

        // Assert
        Assert.True(raised);
    }

    /// <summary>
    /// Tests that the <see cref="UserConfigService.OnConfigChanged"/> event is raised
    /// when a feature is enabled.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task OnConfigChanged_should_BeRaised_when_AFeatureIsEnabled()
    {
        // Arrange
        bool raised = false;
        _service.OnConfigChanged += (_, _) => { raised = true; };
        _userConfigWriterMock.Setup(m => m.Write(It.IsAny<UserConfig>())).Returns(true);

        // Act
        await _service.EnableFeatureAsync(UserFeature.Notification);

        // Assert
        Assert.True(raised);
    }

    /// <summary>
    /// Tests that the <see cref="UserConfigService.OnConfigChanged"/> event is raised
    /// when a feature is disabled.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task OnConfigChanged_should_BeRaised_when_AFeatureIsDisabled()
    {
        // Arrange
        bool raised = false;
        _userConfigWriterMock.Setup(m => m.Write(It.IsAny<UserConfig>())).Returns(true);
        await _service.EnableFeatureAsync(UserFeature.Notification);
        _service.OnConfigChanged += (_, _) => { raised = true; };

        // Act
        await _service.DisableFeatureAsync(UserFeature.Notification);

        // Assert
        Assert.True(raised);
    }

    private async Task SetupMocksAsync(UserConfig userConfig, bool configFileExists, bool successfulSave)
    {
        _fileInfoMock.Setup(f => f.Exists).Returns(configFileExists);
        _fileProviderMock.Setup(m => m.GetFileInfo(AppConfigConstants.UserConfigFileName))
            .Returns(_fileInfoMock.Object);
        _userConfigReaderMock.Setup(r => r.Read()).Returns(userConfig);
        _userConfigWriterMock.Setup(m => m.Write(userConfig)).Returns(successfulSave);
        await _service.StartAsync();
    }
}