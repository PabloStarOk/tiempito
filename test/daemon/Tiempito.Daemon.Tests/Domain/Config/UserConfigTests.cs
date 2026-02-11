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
        _config = new UserConfig(string.Empty);
    }

    /// <summary>
    /// Tests that the constructor of <see cref="UserConfig"/> normalizes the given session ID.
    /// </summary>
    [Fact]
    public void Constructor_should_NormalizeGivenId()
    {
        // Arrange
        const string defaultId = " Default ";
        string expectedId = SessionConfig.NormalizeId(defaultId);

        // Act
        var config = new UserConfig(defaultId);

        // Assert
        Assert.Equal(expectedId, config.DefaultSessionId);
    }

    /// <summary>
    /// Tests that <see cref="UserConfig.SetDefaultSessionConfigId(string)"/> sets the correct normalized session config ID.
    /// </summary>
    [Fact]
    public void SetDefaultSessionConfigId_should_SetCorrectNormalizedId()
    {
        // Arrange
        const string defaultId = " Default ";
        string expectedId = SessionConfig.NormalizeId(defaultId);

        // Act
        _config.SetDefaultSessionConfigId(defaultId);

        // Assert
        Assert.Equal(expectedId, _config.DefaultSessionId);
    }

    /// <summary>
    /// Tests that <see cref="UserConfig.NotificationsEnabled"/> returns true when the Notification feature is enabled.
    /// </summary>
    [Fact]
    public void NotificationsEnabled_should_ReturnTrue_when_FeatureIsEnabled()
    {
        // Act
        _config.EnableFeature(UserFeature.Notification);

        // Assert
        Assert.True(_config.NotificationsEnabled);
    }

    /// <summary>
    /// Tests that <see cref="UserConfig.NotificationsEnabled"/> returns false when the Notification feature is disabled.
    /// </summary>
    [Fact]
    public void NotificationsEnabled_should_ReturnFalse_when_FeatureIsDisabled()
    {
        // Arrange
        _config.EnableFeature(UserFeature.Notification);

        // Act
        _config.DisableFeature(UserFeature.Notification);

        // Assert
        Assert.False(_config.NotificationsEnabled);
    }

    /// <summary>
    /// Tests that <see cref="UserConfig.EnableFeature"/> adds the feature to the enabled features collection.
    /// </summary>
    [Fact]
    public void EnableFeature_should_AddFeatureToEnabledFeaturesCollection()
    {
        // Act
        _config.EnableFeature(UserFeature.Notification);

        // Assert
        Assert.Single(_config.EnabledFeatures);
        Assert.Contains(UserFeature.Notification, _config.EnabledFeatures);
    }

    /// <summary>
    /// Tests that <see cref="UserConfig.EnableFeature"/> does not add duplicated features to the enabled features collection.
    /// </summary>
    [Fact]
    public void EnableFeature_should_NotAddDuplicatedFeaturesToEnabledFeaturesCollection()
    {
        // Arrange
        _config.EnableFeature(UserFeature.Notification);

        // Act
        _config.EnableFeature(UserFeature.Notification);

        // Assert
        Assert.Single(_config.EnabledFeatures);
        Assert.Contains(UserFeature.Notification, _config.EnabledFeatures);
    }

    /// <summary>
    /// Tests that <see cref="UserConfig.DisableFeature"/> removes the feature from the enabled features collection.
    /// </summary>
    [Fact]
    public void DisableFeature_should_RemoveFeatureFromEnabledFeaturesCollection()
    {
        // Arrange
        _config.EnableFeature(UserFeature.Notification);

        // Act
        _config.DisableFeature(UserFeature.Notification);

        // Assert
        Assert.Empty(_config.EnabledFeatures);
    }
}