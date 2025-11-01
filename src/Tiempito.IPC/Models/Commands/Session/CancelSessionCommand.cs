using MessagePack;

namespace Tiempito.IPC.Models.Commands.Session;

/// <summary>
/// Represents a command to cancel a session.
/// </summary>
[MessagePackObject(AllowPrivate = true)]
public sealed record CancelSessionCommand : Command
{
    /// <summary>
    /// Gets the session identifier.
    /// </summary>
    [Key(3)]
    public string SessionId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelSessionCommand"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the command.</param>
    /// <param name="correlationId">The correlation identifier for tracking related messages.</param>
    /// <param name="timestamp">The timestamp when the command was created.</param>
    /// <param name="sessionId">The session identifier.</param>
    internal CancelSessionCommand(
        Guid id,
        Guid correlationId,
        DateTimeOffset timestamp,
        string sessionId)
        : base(id, correlationId, timestamp)
    {
        SessionId = sessionId;
    }

    /// <summary>
    /// Creates a new <see cref="CancelSessionCommand"/> instance.
    /// </summary>
    /// <param name="sessionId">The session identifier. Defaults to an empty string.</param>
    /// <returns>A new <see cref="CancelSessionCommand"/> instance.</returns>
    public static CancelSessionCommand CreateNew(string sessionId = "")
    {
        ArgumentNullException.ThrowIfNull(sessionId);

        return new CancelSessionCommand(
            id: Guid.NewGuid(),
            correlationId: Guid.NewGuid(),
            timestamp: DateTimeOffset.UtcNow,
            sessionId);
    }
}