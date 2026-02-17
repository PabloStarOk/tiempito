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
    private static readonly Dictionary<string, SessionConfig> EmptyDictionary = [];

    private readonly MockRepository _mockRepository;
    private readonly Mock<ISessionConfigWriter> _sessionConfigWriterMock;
    private readonly Mock<IOptionsMonitor<IDictionary<string, SessionConfig>>> _sessionConfigsMonitor;
    private readonly Mock<IOptionsMonitor<UserConfig>> _userConfigOptionsMock;
    private readonly Mock<IDisposable> _userConfigDisposable;
    private readonly Mock<IDisposable> _sessionConfigsDisposable;
    private readonly SessionConfigService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionConfigServiceTests"/> class.
    /// </summary>
    public SessionConfigServiceTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Loose);
        var loggerMock = _mockRepository.Create<ILogger<SessionConfigService>>();
        _sessionConfigWriterMock = _mockRepository.Create<ISessionConfigWriter>();
        _sessionConfigsMonitor = _mockRepository.Create<IOptionsMonitor<IDictionary<string, SessionConfig>>>();
        _userConfigOptionsMock = _mockRepository.Create<IOptionsMonitor<UserConfig>>();
        _userConfigDisposable = _mockRepository.Create<IDisposable>();
        _sessionConfigsDisposable = _mockRepository.Create<IDisposable>();
        _service = new SessionConfigService(
            loggerMock.Object,
            _sessionConfigWriterMock.Object,
            _sessionConfigsMonitor.Object,
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
    public async Task StartAsync_should_SetupConfigurationsAndChangesSubscriptions()
    {
        // Arrange
        var config1 = SessionProvider.CreateRandomConfig(id: "1");
        var config2 = SessionProvider.CreateRandomConfig(id: "2");
        var userConfig = new UserConfig(config2.Id, []);
        _userConfigOptionsMock.Setup(m => m.CurrentValue).Returns(userConfig);
        _sessionConfigsMonitor.Setup(m => m.CurrentValue)
            .Returns(new Dictionary<string, SessionConfig> { { config1.Id, config1 }, { config2.Id, config2 } });

        // Act
        await _service.StartAsync();

        // Assert
        Assert.Equal(config2, _service.DefaultConfig);
        Assert.True(_service.TryGetConfigById(config1.Id, out SessionConfig? actualConfig));
        Assert.NotNull(actualConfig);
        Assert.Equal(config1, actualConfig);
        _userConfigOptionsMock.Verify(m => m.OnChange(It.IsAny<Action<UserConfig, string?>>()), Times.Once);
        _sessionConfigsMonitor.Verify(
            m => m.OnChange(It.IsAny<Action<IDictionary<string, SessionConfig>, string?>>()), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigService.StopAsync"/> disposes the user config changes subscription.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StopAsync_should_DisposeConfigChangesSubscriptions()
    {
        // Arrange
        SetupMocksForStartAsync();
        await _service.StartAsync();

        // Act
        await _service.StopAsync();

        // Assert
        _userConfigDisposable.Verify(m => m.Dispose(), Times.Once);
        _sessionConfigsDisposable.Verify(m => m.Dispose(), Times.Once);
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
        _sessionConfigsMonitor.Setup(m => m.CurrentValue).Returns(EmptyDictionary);
        _sessionConfigWriterMock.Setup(m => m.Write(AppConfigConstants.SessionSectionPrefix, expectedConfig))
            .Returns(true);

        // Act
        OperationResult actual = await _service.AddConfigAsync(expectedConfig);

        // Assert
        Assert.True(actual.Success);
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
        _sessionConfigsMonitor.Setup(m => m.CurrentValue)
            .Returns(new Dictionary<string, SessionConfig> { { initialConfig.NormalizedId, initialConfig } });

        // Act
        OperationResult actual = await _service.AddConfigAsync(invalidConfig);

        // Assert
        Assert.False(actual.Success);
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
        _sessionConfigsMonitor.Setup(m => m.CurrentValue).Returns(EmptyDictionary);
        _sessionConfigWriterMock.Setup(m => m.Write(AppConfigConstants.SessionSectionPrefix, config)).Returns(false);

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
        var modifiedConfig = initialConfig with { TargetCycles = 12, BreakDuration = TimeSpan.FromMinutes(5), };
        _sessionConfigsMonitor.Setup(m => m.CurrentValue)
            .Returns(new Dictionary<string, SessionConfig> { { initialConfig.NormalizedId, initialConfig } });
        _sessionConfigWriterMock.Setup(m => m.Write(AppConfigConstants.SessionSectionPrefix, modifiedConfig))
            .Returns(true);

        // Act
        OperationResult actual = await _service.ModifyConfigAsync(
            initialConfig.Id,
            targetCycles: modifiedConfig.TargetCycles,
            breakDuration: modifiedConfig.BreakDuration);

        // Assert
        Assert.True(actual.Success);
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
        _sessionConfigsMonitor.Setup(m => m.CurrentValue).Returns(EmptyDictionary);

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
        _sessionConfigsMonitor.Setup(m => m.CurrentValue)
            .Returns(new Dictionary<string, SessionConfig> { { initialConfig.Id, initialConfig } });
        _sessionConfigWriterMock.Setup(m => m.Write(AppConfigConstants.SessionSectionPrefix, modifiedConfig))
            .Returns(false);

        // Act
        OperationResult actual = await _service.ModifyConfigAsync(
            initialConfig.Id,
            targetCycles: modifiedConfig.TargetCycles,
            breakDuration: modifiedConfig.BreakDuration);

        // Assert
        Assert.False(actual.Success);
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigService.TryGetConfigById"/> returns the config when the specified ID exists.
    /// </summary>
    [Fact]
    public void TryGetConfigById_should_ReturnConfig_when_IdExists()
    {
        // Arrange
        var expectedConfig = SessionProvider.CreateRandomConfig();
        _sessionConfigsMonitor.Setup(m => m.CurrentValue)
            .Returns(new Dictionary<string, SessionConfig> { { expectedConfig.Id, expectedConfig } });

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
        // Arrange
        _sessionConfigsMonitor.Setup(m => m.CurrentValue).Returns(EmptyDictionary);

        // Act
        bool found = _service.TryGetConfigById("1", out SessionConfig? actualConfig);

        // Assert
        Assert.False(found);
        Assert.Null(actualConfig);
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigService.Dispose"/> disposes the config changes subscriptions.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Dispose_should_DisposeConfigChangesSubscriptions()
    {
        // Arrange
        SetupMocksForStartAsync();
        await _service.StartAsync();

        // Act
        _service.Dispose();

        // Assert
        _userConfigDisposable.Verify(m => m.Dispose(), Times.Once);
        _sessionConfigsDisposable.Verify(m => m.Dispose(), Times.Once);
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
        _sessionConfigsMonitor.Setup(m => m.CurrentValue).Returns(initialConfigs);
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
        _sessionConfigsMonitor.Setup(m => m.CurrentValue).Returns(initialConfigs);
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
        _sessionConfigsMonitor.Setup(m => m.CurrentValue).Returns(initialConfigs);
        await _service.StartAsync();

        // Assert
        Assert.Equal(expectedConfig, _service.DefaultConfig);
    }

    /// <summary>
    /// Tests that the <see cref="SessionConfigService.DefaultConfig"/> is updated when the session configs monitor
    /// callback is invoked and the default config has been modified.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task It_should_UpdateDefaultConfig_when_SessionConfigsMonitorInvokesChangesCallback()
    {
        // Arrange
        var defaultConfig = SessionProvider.CreateRandomConfig();
        var modifiedDefaultConfig = defaultConfig with
        {
            FocusDuration = TimeSpan.FromSeconds(5), BreakDuration = TimeSpan.FromSeconds(2),
        };
        var userConfig = new UserConfig(defaultConfig.Id, []);
        var configs = new List<SessionConfig>
        {
            SessionProvider.CreateRandomConfig(id: "1"),
            SessionProvider.CreateRandomConfig(id: "2"),
            defaultConfig,
        }.ToDictionary(c => c.Id);
        Action<IDictionary<string, SessionConfig>, string?>? onChangeCallback = null;
        _userConfigOptionsMock.Setup(m => m.CurrentValue).Returns(userConfig);
        _sessionConfigsMonitor.Setup(m => m.CurrentValue).Returns(configs);
        _sessionConfigsMonitor.Setup(m => m.OnChange(It.IsAny<Action<IDictionary<string, SessionConfig>, string?>>()))
            .Callback<Action<IDictionary<string, SessionConfig>, string?>>(cb => onChangeCallback = cb);
        await _service.StartAsync();
        configs[defaultConfig.NormalizedId] = modifiedDefaultConfig;

        // Act
        onChangeCallback?.Invoke(configs, null);

        // Assert
        Assert.Equal(modifiedDefaultConfig, _service.DefaultConfig);
    }

    private void SetupMocksForStartAsync()
    {
        _userConfigOptionsMock.Setup(m => m.CurrentValue).Returns(new UserConfig(null, []));
        _userConfigOptionsMock.Setup(m => m.OnChange(It.IsAny<Action<UserConfig, string?>>()))
            .Returns(_userConfigDisposable.Object);
        _sessionConfigsMonitor.Setup(m => m.CurrentValue).Returns(EmptyDictionary);
        _sessionConfigsMonitor.Setup(m => m.OnChange(It.IsAny<Action<IDictionary<string, SessionConfig>, string?>>()))
            .Returns(_sessionConfigsDisposable.Object);
    }
}