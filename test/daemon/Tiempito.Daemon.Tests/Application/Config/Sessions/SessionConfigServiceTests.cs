using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Config.Sessions;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.Daemon.Tests.Sessions.Helpers;

namespace Tiempito.Daemon.Tests.Application.Config.Sessions;

/// <summary>
/// Unit tests for the <see cref="SessionConfigService"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "Config")]
public sealed class SessionConfigServiceTests : IDisposable
{
    private readonly MockRepository _mockRepository;
    private readonly Mock<ISessionConfigWriter> _sessionConfigWriterMock;
    private readonly Mock<ISessionConfigReader> _sessionConfigReaderMock;
    private readonly Mock<IOptionsMonitor<UserConfig>> _userConfigOptionsMock;
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
        _userConfigOptionsMock = _mockRepository.Create<IOptionsMonitor<UserConfig>>();
        _service = new SessionConfigService(
            loggerMock.Object,
            _sessionConfigWriterMock.Object,
            _sessionConfigReaderMock.Object,
            _userConfigOptionsMock.Object);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigService.StartAsync"/> sets up configurations and subscribes to the change events.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StartAsync_should_SetupConfigurationsAndChangesListener()
    {
        // Arrange
        var config1 = SessionProvider.CreateRandomConfig(id: "1");
        var config2 = SessionProvider.CreateRandomConfig(id: "2");
        var userConfig = new UserConfig(config2.Id, []);
        _userConfigOptionsMock.Setup(m => m.CurrentValue).Returns(userConfig);
        _sessionConfigReaderMock.Setup(m => m.ReadSessions(AppConfigConstants.SessionSectionPrefix))
            .Returns(new Dictionary<string, SessionConfig> { { config1.Id, config1 }, { config2.Id, config2 } });

        // Act
        await _service.StartAsync();

        // Assert
        Assert.Equal(config2, _service.DefaultConfig);
        Assert.True(_service.TryGetConfigById(config1.Id, out SessionConfig? actualConfig));
        Assert.NotNull(actualConfig);
        Assert.Equal(config1, actualConfig);
        _userConfigOptionsMock.Verify(m => m.OnChange(It.IsAny<Action<UserConfig, string?>>()), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigService.StopAsync"/> disposes the user config changes subscription.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StopAsync_should_DisposeUserConfigChangesSubscription()
    {
        // Arrange
        var disposable = _mockRepository.Create<IDisposable>();
        var userConfig = new UserConfig(null, []);
        _userConfigOptionsMock.Setup(m => m.CurrentValue).Returns(userConfig);
        _userConfigOptionsMock.Setup(m => m.OnChange(It.IsAny<Action<UserConfig, string?>>()))
            .Returns(disposable.Object);
        _sessionConfigReaderMock.Setup(m => m.ReadSessions(AppConfigConstants.SessionSectionPrefix))
            .Returns(new Dictionary<string, SessionConfig>());
        await _service.StartAsync();

        // Act
        await _service.StopAsync();

        // Assert
        disposable.Verify(m => m.Dispose(), Times.Once);
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
        var userConfig = new UserConfig(null, []);
        var initialConfig = SessionProvider.CreateRandomConfig();
        var modifiedConfig = initialConfig with
        {
            TargetCycles = 12, BreakDuration = TimeSpan.FromMinutes(5),
        };
        _userConfigOptionsMock.Setup(m => m.CurrentValue).Returns(userConfig);
        _sessionConfigWriterMock.Setup(m => m.Write(AppConfigConstants.SessionSectionPrefix, initialConfig))
            .Returns(true);
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
        var userConfig = new UserConfig(initialConfig.Id, []);
        var modifiedConfig = initialConfig with
        {
            FocusDuration = TimeSpan.FromMinutes(5), DelayBetweenTimes = TimeSpan.FromSeconds(2),
        };
        _userConfigOptionsMock.Setup(m => m.CurrentValue).Returns(userConfig);
        _sessionConfigReaderMock.Setup(m => m.ReadSessions(AppConfigConstants.SessionSectionPrefix))
            .Returns(new Dictionary<string, SessionConfig> { { initialConfig.Id, initialConfig } });
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
        var userConfig = new UserConfig(null, []);
        var initialConfig = SessionProvider.CreateRandomConfig();
        var modifiedConfig = initialConfig with
        {
            TargetCycles = 12, BreakDuration = TimeSpan.FromMinutes(5),
        };
        _userConfigOptionsMock.Setup(m => m.CurrentValue).Returns(userConfig);
        _sessionConfigWriterMock.Setup(m => m.Write(AppConfigConstants.SessionSectionPrefix, initialConfig))
            .Returns(true);
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
    /// Tests that the <see cref="SessionConfigService.DefaultConfig"/> is updated when the user config options callback is invoked.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task It_should_UpdateDefaultConfig_when_UserConfigOptionsInvokesCallback()
    {
        // Arrange
        var expectedConfig = SessionProvider.CreateRandomConfig(id: "expected");
        var userConfig = new UserConfig(expectedConfig.Id, []);
        var initialConfigs = new List<SessionConfig>
        {
            SessionProvider.CreateRandomConfig(id: "1"),
            SessionProvider.CreateRandomConfig(id: "2"),
            expectedConfig,
        }.ToDictionary(c => c.Id);
        Action<UserConfig, string?>? onChangeCallback = null;
        _userConfigOptionsMock.Setup(m => m.CurrentValue).Returns(new UserConfig(null, []));
        _userConfigOptionsMock.Setup(m => m.OnChange(It.IsAny<Action<UserConfig, string?>>()))
            .Callback<Action<UserConfig, string?>>(cb => onChangeCallback = cb);
        _sessionConfigReaderMock.Setup(m => m.ReadSessions(AppConfigConstants.SessionSectionPrefix))
            .Returns(initialConfigs);
        await _service.StartAsync();

        // Act
        onChangeCallback?.Invoke(userConfig, null);

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
        var userConfig = new UserConfig(null, []);
        var expectedConfig = SessionProvider.CreateRandomConfig();
        var initialConfigs = new Dictionary<string, SessionConfig>
        {
            { expectedConfig.Id, expectedConfig },
        };
        _userConfigOptionsMock.Setup(m => m.CurrentValue).Returns(userConfig);
        _sessionConfigReaderMock.Setup(m => m.ReadSessions(AppConfigConstants.SessionSectionPrefix))
            .Returns(initialConfigs);
        await _service.StartAsync();

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
        var userConfig = new UserConfig(invalidConfigId, []);
        var expectedConfig = SessionProvider.CreateRandomConfig(id: "expected");
        var initialConfigs = new List<SessionConfig>
        {
            expectedConfig,
            SessionProvider.CreateRandomConfig(id: "1"),
            SessionProvider.CreateRandomConfig(id: "2"),
        }.ToDictionary(c => c.Id);
        _userConfigOptionsMock.Setup(m => m.CurrentValue).Returns(userConfig);
        _sessionConfigReaderMock.Setup(m => m.ReadSessions(AppConfigConstants.SessionSectionPrefix))
            .Returns(initialConfigs);
        await _service.StartAsync();

        // Assert
        Assert.Equal(expectedConfig, _service.DefaultConfig);
    }
}