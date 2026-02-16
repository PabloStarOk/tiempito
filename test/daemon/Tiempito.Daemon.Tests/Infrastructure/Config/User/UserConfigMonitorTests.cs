using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

using Moq;

using Tiempito.Daemon.Application.Config;
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
    private const string DefaultConfigId = "1";
    private const string EnabledFeatures = "notification";

    private readonly MockRepository _mockRepository;
    private readonly Mock<ILogger<UserConfigMonitor>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IConfigurationSection> _defaultConfigIdSectionMock;
    private readonly Mock<IConfigurationSection> _enabledFeaturesSectionMock;
    private readonly Mock<IDisposable> _changeTokenDisposableMock;
    private readonly UserConfigMonitor _monitor;
    private object? _changeCallbackState;
    private Action<object?> _triggerConfigReload = _ => { };

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConfigMonitorTests"/> class.
    /// </summary>
    public UserConfigMonitorTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Loose);
        _loggerMock = _mockRepository.Create<ILogger<UserConfigMonitor>>();
        var changeTokenMock = _mockRepository.Create<IChangeToken>();
        _configurationMock = _mockRepository.Create<IConfiguration>();
        var configSectionMock = _mockRepository.Create<IConfigurationSection>();
        _defaultConfigIdSectionMock = _mockRepository.Create<IConfigurationSection>();
        _enabledFeaturesSectionMock = _mockRepository.Create<IConfigurationSection>();
        _changeTokenDisposableMock = _mockRepository.Create<IDisposable>();

        _configurationMock.Setup(x => x.GetSection(AppConfigConstants.UserSectionName))
            .Returns(configSectionMock.Object);
        configSectionMock.Setup(m => m.GetChildren())
            .Returns([_defaultConfigIdSectionMock.Object, _enabledFeaturesSectionMock.Object]);
        configSectionMock.Setup(m => m.GetSection(nameof(UserConfig.DefaultConfigId)))
            .Returns(_defaultConfigIdSectionMock.Object);
        configSectionMock.Setup(m => m.GetSection(nameof(UserConfig.EnabledFeatures)))
            .Returns(_enabledFeaturesSectionMock.Object);
        _configurationMock.Setup(x => x.GetReloadToken()).Returns(changeTokenMock.Object);
        changeTokenMock
            .Setup(m => m.RegisterChangeCallback(It.IsAny<Action<object?>>(), It.IsAny<object?>()))
            .Callback<Action<object?>, object?>((action, state) =>
            {
                _triggerConfigReload = action;
                _changeCallbackState = state;
            }).Returns(_changeTokenDisposableMock.Object);
        _defaultConfigIdSectionMock.Setup(m => m.Value).Returns(DefaultConfigId);
        _enabledFeaturesSectionMock.Setup(m => m.Value).Returns(EnabledFeatures);
        _monitor = new UserConfigMonitor(_loggerMock.Object, _configurationMock.Object);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
        _monitor.Dispose();
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
        const string defaultConfigId = "1";
        _defaultConfigIdSectionMock.Setup(m => m.Value).Returns(defaultConfigId);
        _enabledFeaturesSectionMock.Setup(m => m.Value).Returns(enabledFeatures);
        var monitor = new UserConfigMonitor(_loggerMock.Object, _configurationMock.Object);

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
    public void Dispose_should_DisposeConfigReloadTokenSubscription()
    {
        // Act
        _monitor.Dispose();

        // Assert
        _changeTokenDisposableMock.Verify(m => m.Dispose(), Times.Once);
    }

    /// <summary>
    /// Ensures that the registered <c>OnChange</c> callback is invoked when the configuration is reloaded.
    /// </summary>
    [Fact]
    public void It_should_InvokeRegisteredOnChangeCallback_when_ConfigIsReloaded()
    {
        // Arrange
        const string newDefaultConfigId = "2";
        string enabledFeatures = string.Empty;
        UserConfig? newConfig = null;
        var invoked = false;
        _defaultConfigIdSectionMock.Setup(m => m.Value).Returns(newDefaultConfigId);
        _enabledFeaturesSectionMock.Setup(m => m.Value).Returns(enabledFeatures);
        _monitor.OnChange((config, _) =>
        {
            invoked = true;
            newConfig = config;
        });

        // Act
        _triggerConfigReload(_changeCallbackState);

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
        _triggerConfigReload(_changeCallbackState);

        // Assert
        Assert.False(invoked);
    }

    /// <summary>
    /// Verifies that the registered <c>OnChange</c> callback is not invoked when the listener is already disposed.
    /// </summary>
    [Fact]
    public void It_should_NotInvokeRegisteredOnChangeCallback_when_ListenerIsAlreadyDisposed()
    {
        // Arrange
        const string newDefaultConfigId = "2";
        string newEnabledFeatures = string.Empty;
        var invoked = false;
        _defaultConfigIdSectionMock.Setup(m => m.Value).Returns(newDefaultConfigId);
        _enabledFeaturesSectionMock.Setup(m => m.Value).Returns(newEnabledFeatures);
        IDisposable onChangeSubscription = _monitor.OnChange((_, _) => invoked = true);
        onChangeSubscription.Dispose();

        // Act
        _triggerConfigReload(_changeCallbackState);

        // Assert
        Assert.False(invoked);
    }
}