using System.Collections.Immutable;

using Tiempito.IPC.Models.Enums;

namespace Tiempito.Daemon.Domain.Config;

/// <summary>
/// Represents the configuration of the user.
/// </summary>
public sealed class UserConfig
{
    /// <summary>
    /// Gets the id of the default session to start by the daemon.
    /// </summary>
    public string? DefaultSessionId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether if the notifications feature is enabled.
    /// </summary>
    public bool NotificationsEnabled => _enabledFeatures.Contains(UserFeature.Notification);

    /// <summary>
    /// Gets all enabled features.
    /// </summary>
    public ImmutableHashSet<UserFeature> EnabledFeatures => _enabledFeatures.ToImmutableHashSet();

    private readonly HashSet<UserFeature> _enabledFeatures = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConfig"/> class.
    /// </summary>
    public UserConfig()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConfig"/> class.
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
    /// Enable the specified feature.
    /// </summary>
    /// <param name="feature">Feature to enable.</param>
    public void EnableFeature(UserFeature feature)
    {
        _enabledFeatures.Add(feature);
    }

    /// <summary>
    /// Disable the specified feature.
    /// </summary>
    /// <param name="feature">Feature to disable.</param>
    public void DisableFeature(UserFeature feature)
    {
        _enabledFeatures.Remove(feature);
    }
}
