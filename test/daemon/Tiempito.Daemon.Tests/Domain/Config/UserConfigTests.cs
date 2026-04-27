using Tiempito.Daemon.Domain.Config;
using Tiempito.IPC.Models.Enums;

namespace Tiempito.Daemon.Tests.Domain.Config;

/// <summary>
/// Unit tests for the <see cref="UserConfig"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "Config")]
public sealed class UserConfigTests
{
    private readonly UserConfig _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConfigTests"/> class.
    /// </summary>
    public UserConfigTests()
    {
        _config = new UserConfig(null, []);
    }

    /// <summary>
    /// Tests that <see cref="UserConfig.NotificationsEnabled"/> returns true when the Notification feature is enabled.
    /// </summary>
    [Fact]
    public void NotificationsEnabled_should_ReturnTrue_when_FeatureIsEnabled()
    {
        // Act
        UserConfig config = _config.WithEnabledFeature(UserFeature.Notification);

        // Assert
        Assert.True(config.NotificationsEnabled);
    }

    /// <summary>
    /// Tests that <see cref="UserConfig.NotificationsEnabled"/> returns false when the Notification feature is disabled.
    /// </summary>
    [Fact]
    public void NotificationsEnabled_should_ReturnFalse_when_FeatureIsDisabled()
    {
        // Arrange
        UserConfig config = _config.WithEnabledFeature(UserFeature.Notification);

        // Act
        config = config.WithDisabledFeature(UserFeature.Notification);

        // Assert
        Assert.False(config.NotificationsEnabled);
    }

    /// <summary>
    /// Tests that <see cref="UserConfig.WithEnabledFeature"/> adds the feature to the enabled features collection.
    /// </summary>
    [Fact]
    public void WithEnabledFeature_should_AddFeatureToEnabledFeaturesCollection()
    {
        // Act
        UserConfig config = _config.WithEnabledFeature(UserFeature.Notification);

        // Assert
        Assert.Single(config.EnabledFeatures);
        Assert.Contains(UserFeature.Notification, config.EnabledFeatures);
    }

    /// <summary>
    /// Tests that <see cref="UserConfig.WithEnabledFeature"/> does not add duplicated features to the enabled features collection.
    /// </summary>
    [Fact]
    public void WithEnabledFeature_should_NotAddDuplicatedFeaturesToEnabledFeaturesCollection()
    {
        // Arrange
        UserConfig config = _config.WithEnabledFeature(UserFeature.Notification);

        // Act
        config = config.WithEnabledFeature(UserFeature.Notification);

        // Assert
        Assert.Single(config.EnabledFeatures);
        Assert.Contains(UserFeature.Notification, config.EnabledFeatures);
    }

    /// <summary>
    /// Tests that <see cref="UserConfig.WithDisabledFeature"/> removes the feature from the enabled features collection.
    /// </summary>
    [Fact]
    public void WithDisabledFeature_should_RemoveFeatureFromEnabledFeaturesCollection()
    {
        // Arrange
        UserConfig config = _config.WithEnabledFeature(UserFeature.Notification);

        // Act
        config = config.WithDisabledFeature(UserFeature.Notification);

        // Assert
        Assert.Empty(config.EnabledFeatures);
    }
}