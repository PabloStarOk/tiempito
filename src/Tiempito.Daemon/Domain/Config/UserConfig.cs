using System.Collections.Immutable;

using Tiempito.IPC.Models.Enums;

namespace Tiempito.Daemon.Domain.Config;

/// <summary>
/// Represents the configuration of the user.
/// </summary>
public sealed class UserConfig
{
    /// <summary>
    /// Gets the id of the default session config to start by the daemon.
    /// </summary>
    public string? DefaultConfigId { get; private set; }

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
    /// <param name="defaultSessionConfigId">ID of the default session config of the user.</param>
    public UserConfig(string defaultSessionConfigId)
    {
        DefaultConfigId = SessionConfig.NormalizeId(defaultSessionConfigId);
    }

    /// <summary>
    /// Sets the default session configuration ID.
    /// </summary>
    /// <param name="id">ID of the session configuration or null if none.</param>
    public void SetDefaultSessionConfigId(string? id)
    {
        if (id is null)
        {
            DefaultConfigId = id;
            return;
        }

        DefaultConfigId = SessionConfig.NormalizeId(id);
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
