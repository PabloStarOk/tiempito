using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Moq;

using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Infrastructure.Config.Sessions;

namespace Tiempito.Daemon.Tests.Infrastructure.Config.Sessions;

/// <summary>
/// Unit tests for the <see cref="SessionConfigsMonitor"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature ", "Config")]
public sealed class SessionConfigsMonitorTests : IDisposable
{
    private const string ConfigId = "Test";
    private const string TargetCycles = "2";
    private const string FocusDuration = "25m";
    private const string BreakDuration = "5m";
    private const string DelayBetweenTimes = "10s";

    private static string TargetCyclesKey => $"Sessions:{ConfigId}:{nameof(SessionConfig.TargetCycles)}";

    private static string FocusDurationKey => $"Sessions:{ConfigId}:{nameof(SessionConfig.FocusDuration)}";

    private static string BreakDurationKey => $"Sessions:{ConfigId}:{nameof(SessionConfig.BreakDuration)}";

    private static string DelayBetweenTimesKey => $"Sessions:{ConfigId}:{nameof(SessionConfig.DelayBetweenTimes)}";

    private static readonly SessionConfig InitialSessionConfig = new (
        ConfigId,
        TargetCycles: 2,
        FocusDuration: TimeSpan.FromMinutes(25),
        BreakDuration: TimeSpan.FromMinutes(5),
        DelayBetweenTimes: TimeSpan.FromSeconds(10));

    private readonly MockRepository _mockRepository;
    private readonly IConfigurationRoot _configurationRoot;
    private readonly Mock<ITimeSpanConverter> _timeSpanConverterMock;
    private readonly SessionConfigsMonitor _monitor;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionConfigsMonitorTests"/> class.
    /// </summary>
    public SessionConfigsMonitorTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Loose);
        var loggerMock = _mockRepository.Create<ILogger<SessionConfigsMonitor>>();
        _timeSpanConverterMock = _mockRepository.Create<ITimeSpanConverter>();
        _timeSpanConverterMock.Setup(m => m.TryConvert(It.IsAny<string>(), out It.Ref<TimeSpan>.IsAny))
            .Returns(new TryConvertDelegate((string input, out TimeSpan result) =>
            {
                result = input switch
                {
                    FocusDuration => TimeSpan.FromMinutes(25),
                    BreakDuration => TimeSpan.FromMinutes(5),
                    DelayBetweenTimes => TimeSpan.FromSeconds(10),
                    _ => TimeSpan.Zero,
                };
                return true;
            }));

        var initialConfig = new Dictionary<string, string?>
        {
            { TargetCyclesKey, TargetCycles },
            { FocusDurationKey, FocusDuration },
            { BreakDurationKey, BreakDuration },
            { DelayBetweenTimesKey, DelayBetweenTimes },
        };

        _configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(initialConfig)
            .Build();

        _monitor = new SessionConfigsMonitor(
            loggerMock.Object, _configurationRoot, _timeSpanConverterMock.Object);
    }

    private delegate bool TryConvertDelegate(string input, out TimeSpan result);

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
        _monitor.Dispose();
        (_configurationRoot as IDisposable)?.Dispose();
    }

    /// <summary>
    /// Verifies that the <see cref="SessionConfigsMonitor.CurrentValue"/> property returns the current session configurations.
    /// </summary>
    [Fact]
    public void CurrentValue_should_ReturnCurrentSessionConfigs()
    {
        // Assert
        Assert.Single(_monitor.CurrentValue);
        Assert.True(_monitor.CurrentValue.TryGetValue(InitialSessionConfig.NormalizedId, out SessionConfig? config));
        Assert.NotNull(config);
        Assert.Equal(InitialSessionConfig, config);
    }

    /// <summary>
    /// Tests that the <see cref="SessionConfigsMonitor.Get"/> method returns the current value
    /// and ignores the provided options name.
    /// </summary>
    [Fact]
    public void Get_should_ReturnCurrentValueAndIgnoreGivenOptionsName()
    {
        // Arrange
        const string optionsName = "RandomName";

        // Act
        var actual = _monitor.Get(optionsName);

        // Assert
        Assert.Equal(_monitor.CurrentValue, actual);
    }

    /// <summary>
    /// Ensures that after disposing the <see cref="SessionConfigsMonitor"/>, it stops listening to configuration changes.
    /// </summary>
    [Fact]
    public void Dispose_should_StopListeningToConfigChanges()
    {
        // Arrange
        bool invoked = false;
        _monitor.OnChange((_, _) => invoked = true);
        _configurationRoot[TargetCyclesKey] = "99";

        // Act
        _monitor.Dispose();
        _configurationRoot.Reload();

        // Assert
        Assert.False(invoked);
    }

    /// <summary>
    /// Verifies that the registered OnChange callback is invoked when the configuration is reloaded and session configs change.
    /// </summary>
    [Fact]
    public void It_should_InvokeRegisteredOnChangeCallback_when_ConfigIsReloaded()
    {
        // Arrange
        bool invoked = false;
        SessionConfig? actualSessionConfig = null;
        var expectedSessionConfig = InitialSessionConfig with
        {
            FocusDuration = TimeSpan.FromSeconds(15),
            BreakDuration = TimeSpan.FromSeconds(15),
            DelayBetweenTimes = TimeSpan.FromSeconds(15),
        };
        _configurationRoot[FocusDurationKey] = "15s";
        _configurationRoot[BreakDurationKey] = "15s";
        _configurationRoot[DelayBetweenTimesKey] = "15s";
        _timeSpanConverterMock.Setup(m => m.TryConvert(It.IsAny<string>(), out It.Ref<TimeSpan>.IsAny))
             .Returns(new TryConvertDelegate((string _, out TimeSpan result) =>
             {
                 result = TimeSpan.FromSeconds(15);
                 return true;
             }));
        _monitor.OnChange((configs, _) =>
        {
            invoked = true;
            actualSessionConfig = configs.Values.FirstOrDefault();
        });

        // Act
        _configurationRoot.Reload();

        // Assert
        Assert.True(invoked);
        Assert.NotNull(actualSessionConfig);
        Assert.Equal(expectedSessionConfig, actualSessionConfig);
    }

    /// <summary>
    /// Ensures that the OnChange callback is not invoked when the configuration is reloaded
    /// and the session configurations have not changed.
    /// </summary>
    [Fact]
    public void It_should_NotInvokeRegisteredOnChangeCallback_when_ConfigIsReloadedAndSessionConfigsDoNotChange()
    {
        // Arrange
        bool invoked = false;
        _monitor.OnChange((_, _) => invoked = true);

        // Act
        _configurationRoot.Reload();

        // Assert
        Assert.False(invoked);
    }

    /// <summary>
    /// Ensures that the OnChange callback is not invoked when the subscription has already been disposed.
    /// </summary>
    [Fact]
    public void It_should_NotInvokeRegisteredOnChangeCallback_when_SubscriptionIsAlreadyDisposed()
    {
        // Arrange
        bool invoked = false;
        IDisposable subscription = _monitor.OnChange((_, _) => invoked = true);
        subscription.Dispose();
        _configurationRoot[TargetCyclesKey] = "99";

        // Act
        _configurationRoot.Reload();

        // Assert
        Assert.False(invoked);
    }

    /// <summary>
    /// Verifies that session configurations are ignored when either FocusDuration or BreakDuration are invalid.
    /// </summary>
    [Fact]
    public void It_should_IgnoreSessionConfigs_when_FocusOrBreakDurationAreInvalid()
    {
        // Arrange
        _timeSpanConverterMock.Setup(m => m.TryConvert(It.IsAny<string>(), out It.Ref<TimeSpan>.IsAny)).Returns(false);

        // Act
        _configurationRoot.Reload();

        // Assert
        Assert.Empty(_monitor.CurrentValue);
    }

    /// <summary>
    /// Ensures that session configurations are ignored when their TargetCycles are invalid.
    /// </summary>
    [Fact]
    public void It_should_IgnoreSessionConfigs_when_TargetCyclesIsInvalid()
    {
        // Arrange
        _configurationRoot[TargetCyclesKey] = "Invalid";

        // Act
        _configurationRoot.Reload();

        // Assert
        Assert.Empty(_monitor.CurrentValue);
    }
}