using Microsoft.Extensions.Logging;

using Moq;

using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Config.Sessions;
using Tiempito.Daemon.Application.Config.User;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.Daemon.Tests.Sessions.Helpers;

namespace Tiempito.Daemon.Tests.Application.Config.Sessions;

/// <summary>
/// Unit tests for the <see cref="SessionConfigService"/> class.
/// </summary>
[Trait("Config", "Unit")]
public sealed class SessionConfigServiceTests : IDisposable
{
    private readonly MockRepository _mockRepository;
    private readonly Mock<ISessionConfigWriter> _sessionConfigWriterMock;
    private readonly Mock<ISessionConfigReader> _sessionConfigReaderMock;
    private readonly Mock<IUserConfigService> _userConfigServiceMock;
    private readonly UserConfig _userConfig;
    private readonly SessionConfigService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionConfigServiceTests"/> class.
    /// </summary>
    public SessionConfigServiceTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Loose);
        var loggerMock = _mockRepository.Create<ILogger<SessionConfigService>>();
        _sessionConfigWriterMock = _mockRepository.Create<ISessionConfigWriter>();
        _sessionConfigReaderMock = _mockRepository.Create<ISessionConfigReader>();
        _userConfigServiceMock = _mockRepository.Create<IUserConfigService>();
        _userConfig = new UserConfig();
        _service = new SessionConfigService(
            loggerMock.Object,
            _userConfigServiceMock.Object,
            _sessionConfigWriterMock.Object,
            _sessionConfigReaderMock.Object);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigService.StartAsync"/> sets up configurations and subscribes to the event listener.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StartAsync_should_SetupConfigurationsAndEventListener()
    {
        // Arrange
        var config1 = SessionProvider.CreateRandomConfig(id: "1");
        var config2 = SessionProvider.CreateRandomConfig(id: "2");
        _userConfig.SetDefaultSessionConfigId(config2.Id);
        _userConfigServiceMock.Setup(m => m.UserConfig).Returns(_userConfig);
        _sessionConfigReaderMock.Setup(m => m.ReadSessions(AppConfigConstants.SessionSectionPrefix))
            .Returns(new Dictionary<string, SessionConfig> { { config1.Id, config1 }, { config2.Id, config2 } });

        // Act
        await _service.StartAsync();

        // Assert
        Assert.Equal(config2, _service.DefaultConfig);
        Assert.True(_service.TryGetConfigById(config1.Id, out SessionConfig? actualConfig));
        Assert.NotNull(actualConfig);
        Assert.Equal(config1, actualConfig);
        _userConfigServiceMock.VerifyAdd(m => m.OnConfigChanged += It.IsAny<EventHandler>(), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigService.StopAsync"/> unsubscribes from the user config changed event.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StopAsync_should_UnsubscribeFromUserConfigChangedEvent()
    {
        // Act
        await _service.StopAsync();

        // Assert
        _userConfigServiceMock.VerifyRemove(m => m.OnConfigChanged -= It.IsAny<EventHandler>(), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigService.AddConfigAsync"/> returns success when the config is valid.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddConfigAsync_should_ReturnSuccess_when_ConfigIsValid()
    {
        // Arrange
        var expectedConfig = SessionProvider.CreateRandomConfig();
        _sessionConfigWriterMock.Setup(m => m.Write(AppConfigConstants.SessionSectionPrefix, expectedConfig))
            .Returns(true);

        // Act
        OperationResult actual = await _service.AddConfigAsync(expectedConfig);

        // Assert
        Assert.True(actual.Success);
        Assert.True(_service.TryGetConfigById(expectedConfig.Id, out SessionConfig? actualConfig));
        Assert.NotNull(actualConfig);
        Assert.Equal(expectedConfig, actualConfig);
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigService.AddConfigAsync"/> returns an error when the config ID already exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddConfigAsync_should_ReturnError_when_IdAlreadyExists()
    {
        // Arrange
        var initialConfig = SessionProvider.CreateRandomConfig();
        var invalidConfig = SessionProvider.CreateRandomConfig() with { Id = initialConfig.Id };
        _sessionConfigWriterMock.Setup(m => m.Write(AppConfigConstants.SessionSectionPrefix, It.IsAny<SessionConfig>()))
            .Returns(true);
        await _service.AddConfigAsync(initialConfig);

        // Act
        OperationResult actual = await _service.AddConfigAsync(invalidConfig);

        // Assert
        Assert.False(actual.Success);
        Assert.True(_service.TryGetConfigById(initialConfig.Id, out SessionConfig? actualConfig));
        Assert.NotNull(actualConfig);
        Assert.Equal(initialConfig, actualConfig);
        Assert.NotEqual(invalidConfig, actualConfig);
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigService.AddConfigAsync"/> returns an error when the save operation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddConfigAsync_should_ReturnError_when_SaveOperationFails()
    {
        // Arrange
        var config = SessionProvider.CreateRandomConfig();
        _sessionConfigWriterMock.Setup(m => m.Write(AppConfigConstants.SessionSectionPrefix, config))
            .Returns(false);

        // Act
        OperationResult actual = await _service.AddConfigAsync(config);

        // Assert
        Assert.False(actual.Success);
        Assert.False(_service.TryGetConfigById(config.Id, out SessionConfig? actualConfig));
        Assert.Null(actualConfig);
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigService.ModifyConfigAsync"/> returns success when the config is valid.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModifyConfigAsync_should_ReturnSuccess_when_ConfigIsValid()
    {
        // Arrange
        var initialConfig = SessionProvider.CreateRandomConfig();
        var modifiedConfig = initialConfig with
        {
            TargetCycles = 12, BreakDuration = TimeSpan.FromMinutes(5),
        };
        _sessionConfigWriterMock.Setup(m => m.Write(AppConfigConstants.SessionSectionPrefix, initialConfig))
            .Returns(true);
        _userConfigServiceMock.Setup(m => m.UserConfig).Returns(_userConfig);
        await _service.AddConfigAsync(initialConfig);
        _sessionConfigWriterMock.Setup(m => m.Write(AppConfigConstants.SessionSectionPrefix, modifiedConfig))
            .Returns(true);

        // Act
        OperationResult actual = await _service.ModifyConfigAsync(
            initialConfig.Id,
            targetCycles: modifiedConfig.TargetCycles,
            breakDuration: modifiedConfig.BreakDuration);

        // Assert
        Assert.True(actual.Success);
        Assert.True(_service.TryGetConfigById(initialConfig.Id, out SessionConfig? actualConfig));
        Assert.Equal(modifiedConfig, actualConfig);
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigService.ModifyConfigAsync"/> updates the default config when the modified config is the default.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModifyConfigAsync_should_UpdateDefaultConfig_when_ModifiedConfigIsDefault()
    {
        // Arrange
        var initialConfig = SessionProvider.CreateRandomConfig();
        var modifiedConfig = initialConfig with
        {
            FocusDuration = TimeSpan.FromMinutes(5), DelayBetweenTimes = TimeSpan.FromSeconds(2),
        };
        _userConfig.SetDefaultSessionConfigId(initialConfig.Id);
        _sessionConfigReaderMock.Setup(m => m.ReadSessions(AppConfigConstants.SessionSectionPrefix))
            .Returns(new Dictionary<string, SessionConfig> { { initialConfig.Id, initialConfig } });
        _userConfigServiceMock.Setup(m => m.UserConfig).Returns(_userConfig);
        _sessionConfigWriterMock.Setup(m => m.Write(AppConfigConstants.SessionSectionPrefix, modifiedConfig))
            .Returns(true);
        await _service.StartAsync();

        // Act
        OperationResult actual = await _service.ModifyConfigAsync(
            initialConfig.Id,
            focusDuration: modifiedConfig.FocusDuration,
            delayBetweenTimes: modifiedConfig.DelayBetweenTimes);

        // Assert
        Assert.True(actual.Success);
        Assert.Equal(modifiedConfig, _service.DefaultConfig);
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigService.ModifyConfigAsync"/> returns an error when the specified config ID does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModifyConfigAsync_should_ReturnError_when_IdDoesNotExist()
    {
        // Arrange
        var config = SessionProvider.CreateRandomConfig();

        // Act
        OperationResult actual = await _service.ModifyConfigAsync(config.Id, focusDuration: TimeSpan.FromMinutes(5));

        // Assert
        Assert.False(actual.Success);
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigService.ModifyConfigAsync"/> returns an error when the save operation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModifyConfigAsync_should_ReturnError_when_SaveOperationFails()
    {
        // Arrange
        var initialConfig = SessionProvider.CreateRandomConfig();
        var modifiedConfig = initialConfig with
        {
            TargetCycles = 12, BreakDuration = TimeSpan.FromMinutes(5),
        };
        _sessionConfigWriterMock.Setup(m => m.Write(AppConfigConstants.SessionSectionPrefix, initialConfig))
            .Returns(true);
        _userConfigServiceMock.Setup(m => m.UserConfig).Returns(_userConfig);
        await _service.AddConfigAsync(initialConfig);
        _sessionConfigWriterMock.Setup(m => m.Write(AppConfigConstants.SessionSectionPrefix, modifiedConfig))
            .Returns(false);

        // Act
        OperationResult actual = await _service.ModifyConfigAsync(
            initialConfig.Id,
            targetCycles: modifiedConfig.TargetCycles,
            breakDuration: modifiedConfig.BreakDuration);

        // Assert
        Assert.False(actual.Success);
        Assert.True(_service.TryGetConfigById(initialConfig.Id, out SessionConfig? actualConfig));
        Assert.Equal(initialConfig, actualConfig);
        Assert.NotEqual(modifiedConfig, actualConfig);
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigService.TryGetConfigById"/> returns the config when the specified ID exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task TryGetConfigById_should_ReturnConfig_when_IdExists()
    {
        // Arrange
        var expectedConfig = SessionProvider.CreateRandomConfig();
        _sessionConfigWriterMock.Setup(m => m.Write(AppConfigConstants.SessionSectionPrefix, expectedConfig))
            .Returns(true);
        await _service.AddConfigAsync(expectedConfig);

        // Act
        bool found = _service.TryGetConfigById(expectedConfig.Id, out SessionConfig? actualConfig);

        // Assert
        Assert.True(found);
        Assert.Equal(expectedConfig, actualConfig);
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigService.TryGetConfigById"/> returns false when the specified ID does not exist.
    /// </summary>
    [Fact]
    public void TryGetConfigById_should_ReturnFalse_when_IdDoesNotExist()
    {
        // Act
        bool found = _service.TryGetConfigById("1", out SessionConfig? actualConfig);

        // Assert
        Assert.False(found);
        Assert.Null(actualConfig);
    }

    /// <summary>
    /// Tests that the <see cref="SessionConfigService.DefaultConfig"/> is refreshed when the <see cref="IUserConfigService.OnConfigChanged"/> event is raised.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DefaultConfig_should_Refresh_when_UserConfigServiceRaisesChangedEvent()
    {
        // Arrange
        var expectedConfig = SessionProvider.CreateRandomConfig(id: "expected");
        var initialConfigs = new List<SessionConfig>
        {
            SessionProvider.CreateRandomConfig(id: "1"),
            SessionProvider.CreateRandomConfig(id: "2"),
            expectedConfig,
        }.ToDictionary(c => c.Id);
        _userConfigServiceMock.Setup(m => m.UserConfig).Returns(_userConfig);
        _sessionConfigReaderMock.Setup(m => m.ReadSessions(AppConfigConstants.SessionSectionPrefix))
            .Returns(initialConfigs);
        await _service.StartAsync();

        // Act
        _userConfig.SetDefaultSessionConfigId(expectedConfig.Id);
        _userConfigServiceMock.Raise(m => m.OnConfigChanged += null, EventArgs.Empty);

        // Assert
        Assert.Equal(expectedConfig, _service.DefaultConfig);
    }

    /// <summary>
    /// Tests that the <see cref="SessionConfigService.DefaultConfig"/> uses the Pomodoro config as a fallback when there are no session configs available.
    /// </summary>
    [Fact]
    public void DefaultConfig_should_UsePomodoroConfigAsFallback_when_ThereAreNoConfigs()
    {
        var pomoConfig = new SessionConfig(
            Id: "Default",
            TargetCycles: 4,
            DelayBetweenTimes: TimeSpan.FromSeconds(10),
            FocusDuration: TimeSpan.FromMinutes(25),
            BreakDuration: TimeSpan.FromMinutes(5));

        // Assert
        Assert.Equal(pomoConfig, _service.DefaultConfig);
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigService.DefaultConfig"/> is set to the first config when the config dictionary contains only one entry.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DefaultConfig_should_BeFirstConfig_when_ConfigDictionaryIsSingle()
    {
        // Arrange
        var expectedConfig = SessionProvider.CreateRandomConfig();
        _userConfigServiceMock.Setup(m => m.UserConfig).Returns(_userConfig);
        _sessionConfigReaderMock.Setup(m => m.ReadSessions(AppConfigConstants.SessionSectionPrefix))
            .Returns(new Dictionary<string, SessionConfig> { { expectedConfig.Id, expectedConfig } });
        await _service.StartAsync();

        // Act
        _userConfigServiceMock.Raise(m => m.OnConfigChanged += null, EventArgs.Empty);

        // Assert
        Assert.Equal(expectedConfig, _service.DefaultConfig);
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigService.DefaultConfig"/> is set to the first config when the user's default config ID is invalid.
    /// </summary>
    /// <param name="invalidConfigId">The invalid default config ID to test.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("3")]
    [InlineData(null)]
    public async Task DefaultConfig_should_BeFirstConfig_when_UserDefaultIdIsInvalid(
        string? invalidConfigId)
    {
        // Arrange
        var expectedConfig = SessionProvider.CreateRandomConfig(id: "expected");
        var initialConfigs = new List<SessionConfig>
        {
            expectedConfig,
            SessionProvider.CreateRandomConfig(id: "1"),
            SessionProvider.CreateRandomConfig(id: "2"),
        }.ToDictionary(c => c.Id);
        _userConfigServiceMock.Setup(m => m.UserConfig).Returns(_userConfig);
        _sessionConfigReaderMock.Setup(m => m.ReadSessions(AppConfigConstants.SessionSectionPrefix))
            .Returns(initialConfigs);
        await _service.StartAsync();

        // Act
        _userConfig.SetDefaultSessionConfigId(invalidConfigId);
        _userConfigServiceMock.Raise(m => m.OnConfigChanged += null, EventArgs.Empty);

        // Assert
        Assert.Equal(expectedConfig, _service.DefaultConfig);
    }
}