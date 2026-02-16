using System.Collections.Immutable;

using Tiempito.IPC.Models.Enums;

namespace Tiempito.Daemon.Domain.Config;

/// <summary>
/// Represents the configuration of the user.
/// </summary>
public sealed record UserConfig(string? DefaultConfigId, ImmutableHashSet<UserFeature> EnabledFeatures)
{
    /// <summary>
    /// Gets a value indicating whether if the notifications feature is enabled.
    /// </summary>
    public bool NotificationsEnabled => EnabledFeatures.Contains(UserFeature.Notification);

    /// <summary>
    /// Returns a new <see cref="UserConfig"/> instance with the specified feature enabled.
    /// </summary>
    /// <param name="feature">The feature to enable.</param>
    /// <returns>A new <see cref="UserConfig"/> with the feature enabled.</returns>
    public UserConfig WithEnabledFeature(UserFeature feature)
    {
        return this with
        {
            EnabledFeatures = EnabledFeatures.Add(feature),
        };
    }

    /// <summary>
    /// Returns a new <see cref="UserConfig"/> instance with the specified feature disabled.
    /// </summary>
    /// <param name="feature">The feature to disable.</param>
    /// <returns>A new <see cref="UserConfig"/> with the feature disabled.</returns>
    public UserConfig WithDisabledFeature(UserFeature feature)
    {
        return this with
        {
            EnabledFeatures = EnabledFeatures.Remove(feature),
        };
    }
}
