using Tiempito.IPC.Models.Enums;

namespace Tiempito.Daemon.Domain.Config;

/// <summary>
/// Represents the configuration of the user.
/// </summary>
public struct UserConfig
{
    /// <summary>
    /// Gets the id of the default session to start by the daemon.
    /// </summary>
    public string DefaultSessionId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether if the notifications feature is enabled.
    /// </summary>
    public bool NotificationsEnabled => _enabledFeatures.Contains(UserFeature.Notification);

    /// <summary>
    /// Gets all enabled features.
    /// </summary>
    public IReadOnlyList<UserFeature> EnabledFeatures => _enabledFeatures;

    private readonly List<UserFeature> _enabledFeatures = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConfig"/> struct.
    /// </summary>
    public UserConfig()
    {
        DefaultSessionId = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConfig"/> struct.
    /// </summary>
    /// <param name="defaultSessionId">ID of the default session of the user.</param>
    public UserConfig(string defaultSessionId)
    {
        DefaultSessionId = SessionConfig.NormalizeId(defaultSessionId);
    }

    /// <summary>
    /// Sets the default session configuration ID.
    /// </summary>
    /// <param name="id">ID of the session configuration.</param>
    public void SetDefaultSessionConfigId(string id)
    {
        DefaultSessionId = SessionConfig.NormalizeId(id);
    }

    /// <summary>
    /// Add the given feature to the enabled ones.
    /// </summary>
    /// <param name="feature">Feature to add.</param>
    public void AddFeature(UserFeature feature)
    {
        _enabledFeatures.Add(feature);
    }

    /// <summary>
    /// Remove a feature from the enabled ones.
    /// </summary>
    /// <param name="feature">Feature to remove.</param>
    public void RemoveFeature(UserFeature feature)
    {
        _enabledFeatures.Remove(feature);
    }
}
