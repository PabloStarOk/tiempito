using MessagePack;

using Tiempito.IPC.Models.Enums;

namespace Tiempito.IPC.Models.Commands.Config;

/// <summary>
/// Represents to enable or disable feature.
/// </summary>
[MessagePackObject(AllowPrivate = true)]
public sealed record UserFeatureConfigCommand : Command
{
    /// <summary>
    /// Gets a value indicating whether to enable or disable the feature.
    /// </summary>
    [Key(3)]
    public bool Enable { get; }

    /// <summary>
    /// Gets the user feature to be enabled/disabled.
    /// </summary>
    [Key(4)]
    public UserFeature Feature { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserFeatureConfigCommand"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the command.</param>
    /// <param name="correlationId">The correlation identifier for tracking related messages.</param>
    /// <param name="timestamp">The timestamp when the command was created.</param>
    /// <param name="enable">Indicates whether to enable or disable the feature.</param>
    /// <param name="feature">The user feature to be enabled or disabled.</param>
    internal UserFeatureConfigCommand(
        Guid id,
        Guid correlationId,
        DateTimeOffset timestamp,
        bool enable,
        UserFeature feature)
        : base(id, correlationId, timestamp)
    {
        Enable = enable;
        Feature = feature;
    }

    /// <summary>
    /// Creates a new instance of <see cref="UserFeatureConfigCommand"/> with a unique identifier,
    /// correlation ID, and the current UTC timestamp.
    /// </summary>
    /// <param name="enable">Indicates whether to enable or disable the feature.</param>
    /// <param name="feature">The user feature to be enabled or disabled.</param>
    /// <returns>A new <see cref="UserFeatureConfigCommand"/> instance.</returns>
    public static UserFeatureConfigCommand CreateNew(bool enable, UserFeature feature)
    {
        ArgumentNullException.ThrowIfNull(enable);
        ArgumentNullException.ThrowIfNull(feature);

        return new UserFeatureConfigCommand(
            id: Guid.NewGuid(),
            correlationId: Guid.NewGuid(),
            timestamp: DateTimeOffset.UtcNow,
            enable,
            feature);
    }
}