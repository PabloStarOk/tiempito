using MessagePack;

namespace Tiempito.IPC.Models.Commands.Config;

/// <summary>
/// Represents a command to set the default session configuration.
/// </summary>
[MessagePackObject(AllowPrivate = true)]
public sealed record SetConfigCommand : Command
{
    /// <summary>
    /// Gets the identifier of the default session configuration to be set.
    /// </summary>
    [Key(3)]
    public string DefaultSessionConfigId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SetConfigCommand"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the command.</param>
    /// <param name="correlationId">The correlation identifier for tracking related messages.</param>
    /// <param name="timestamp">The timestamp when the command was created.</param>
    /// <param name="defaultSessionConfigId">The identifier of the default session configuration to set.</param>
    internal SetConfigCommand(
        Guid id,
        Guid correlationId,
        DateTimeOffset timestamp,
        string defaultSessionConfigId)
        : base(id, correlationId, timestamp)
    {
        DefaultSessionConfigId = defaultSessionConfigId;
    }

    /// <summary>
    /// Creates a new <see cref="SetConfigCommand"/> instance with a generated ID, correlation ID, and current timestamp.
    /// </summary>
    /// <param name="defaultSessionConfigId">The identifier of the default session configuration to set.</param>
    /// <returns>A new <see cref="SetConfigCommand"/> instance.</returns>
    public static SetConfigCommand CreateNew(string defaultSessionConfigId)
    {
        ArgumentNullException.ThrowIfNull(defaultSessionConfigId);

        return new SetConfigCommand(
            id: Guid.NewGuid(),
            correlationId: Guid.NewGuid(),
            timestamp: DateTimeOffset.UtcNow,
            defaultSessionConfigId);
    }
}