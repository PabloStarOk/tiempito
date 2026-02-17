using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Moq;

using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Infrastructure.Config.User;
using Tiempito.IPC.Models.Enums;

namespace Tiempito.Daemon.Tests.Infrastructure.Config.User;

/// <summary>
/// Unit tests for <see cref="UserConfigMonitor"/>.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "Config")]
public sealed class UserConfigMonitorTests : IDisposable
{
    private const string DefaultConfigIdKey = "User:DefaultConfigId";
    private const string EnabledFeaturesKey = "User:EnabledFeatures";
    private const string DefaultConfigId = "1";
    private const string EnabledFeatures = "notification";

    private readonly MockRepository _mockRepository;
    private readonly Mock<ILogger<UserConfigMonitor>> _loggerMock;
    private readonly IConfigurationRoot _configurationRoot;
    private readonly UserConfigMonitor _monitor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConfigMonitorTests"/> class.
    /// </summary>
    public UserConfigMonitorTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Loose);
        _loggerMock = _mockRepository.Create<ILogger<UserConfigMonitor>>();

        var initialConfig = new Dictionary<string, string?>
        {
            { DefaultConfigIdKey, DefaultConfigId },
            { EnabledFeaturesKey, EnabledFeatures },
        };

        _configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(initialConfig)
            .Build();

        _monitor = new UserConfigMonitor(_loggerMock.Object, _configurationRoot);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
        _monitor.Dispose();
        (_configurationRoot as IDisposable)?.Dispose();
    }

    /// <summary>
    /// Verifies that <see cref="UserConfigMonitor.CurrentValue"/> returns the correct <see cref="UserConfig"/>
    /// when different enabled features are provided, including case-insensitive and invalid feature names.
    /// </summary>
    /// <param name="enabledFeatures">A comma-separated string of enabled feature names.</param>
    [Theory]
    [InlineData("nOtIfiCAtiOn")]
    [InlineData("nOtIfiCAtiOn,InvalidFeature,InvalidFeature2")]
    public void CurrentValue_should_ReturnCurrentUserConfig(string enabledFeatures)
    {
        // Arrange
        _configurationRoot[DefaultConfigIdKey] = DefaultConfigId;
        _configurationRoot[EnabledFeaturesKey] = enabledFeatures;
        var monitor = new UserConfigMonitor(_loggerMock.Object, _configurationRoot);

        // Act
        var actual = monitor.CurrentValue;

        // Assert
        Assert.Equal(DefaultConfigId, actual.DefaultConfigId);
        Assert.Single(actual.EnabledFeatures);
        Assert.Contains(UserFeature.Notification, actual.EnabledFeatures);
    }

    /// <summary>
    /// Verifies that <see cref="UserConfigMonitor.Get"/> returns the current value and ignores the provided options name.
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
    /// Verifies that disposing the <see cref="UserConfigMonitor"/> disposes the configuration reload token subscription.
    /// </summary>
    [Fact]
    public void Dispose_should_StopListeningToConfigChanges()
    {
        // Arrange
        bool invoked = false;
        _monitor.OnChange((_, _) => invoked = true);
        _configurationRoot[DefaultConfigIdKey] = "3";

        // Act
        _monitor.Dispose();
        _configurationRoot.Reload();

        // Assert
        Assert.False(invoked);
    }

    /// <summary>
    /// Ensures that the registered <c>OnChange</c> callback is invoked when the configuration is reloaded.
    /// </summary>
    [Fact]
    public void It_should_InvokeRegisteredOnChangeCallback_when_ConfigIsReloaded()
    {
        // Arrange
        const string newDefaultConfigId = "2";
        string newEnabledFeatures = string.Empty;
        UserConfig? newConfig = null;
        var invoked = false;
        _configurationRoot[DefaultConfigIdKey] = newDefaultConfigId;
        _configurationRoot[EnabledFeaturesKey] = newEnabledFeatures;
        _monitor.OnChange((config, _) =>
        {
            invoked = true;
            newConfig = config;
        });

        // Act
        _configurationRoot.Reload();

        // Assert
        Assert.True(invoked);
        Assert.NotNull(newConfig);
        Assert.Equal(newDefaultConfigId, newConfig.DefaultConfigId);
        Assert.Equal(_monitor.CurrentValue, newConfig);
    }

    /// <summary>
    /// Verifies that the registered <c>OnChange</c> callback is not invoked when the configuration is reloaded
    /// and the <see cref="UserConfig"/> does not change.
    /// </summary>
    [Fact]
    public void It_should_NotInvokeRegisteredOnChangeCallback_when_ConfigIsReloadedAndUserConfigDoesNotChange()
    {
         // Arrange
        var invoked = false;
        _monitor.OnChange((_, _) => invoked = true);

        // Act
        _configurationRoot.Reload();

        // Assert
        Assert.False(invoked);
    }

    /// <summary>
    /// Verifies that the registered <c>OnChange</c> callback is not invoked when the subscription is already disposed.
    /// </summary>
    [Fact]
    public void It_should_NotInvokeRegisteredOnChangeCallback_when_SubscriptionIsAlreadyDisposed()
    {
        // Arrange
        const string newDefaultConfigId = "2";
        string newEnabledFeatures = string.Empty;
        var invoked = false;
        _configurationRoot[DefaultConfigIdKey] = newDefaultConfigId;
        _configurationRoot[EnabledFeaturesKey] = newEnabledFeatures;
        IDisposable onChangeSubscription = _monitor.OnChange((_, _) => invoked = true);
        onChangeSubscription.Dispose();

        // Act
        _configurationRoot.Reload();

        // Assert
        Assert.False(invoked);
    }
}