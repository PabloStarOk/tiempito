using MessagePack;

namespace Tiempito.IPC.Models.Commands.Session;

/// <summary>
/// Represents a command to pause a session.
/// </summary>
[MessagePackObject(AllowPrivate = true)]
public sealed record PauseSessionCommand : Command
{
    /// <summary>
    /// Gets the session identifier.
    /// </summary>
    [Key(3)]
    public string SessionId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PauseSessionCommand"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the command.</param>
    /// <param name="correlationId">The correlation identifier for tracking related messages.</param>
    /// <param name="timestamp">The timestamp when the command was created.</param>
    /// <param name="sessionId">The session identifier.</param>
    internal PauseSessionCommand(
        Guid id,
        Guid correlationId,
        DateTimeOffset timestamp,
        string sessionId)
        : base(id, correlationId, timestamp)
    {
        SessionId = sessionId;
    }

    /// <summary>
    /// Creates a new <see cref="PauseSessionCommand"/> instance.
    /// </summary>
    /// <param name="sessionId">The session identifier. Defaults to an empty string.</param>
    /// <returns>A new <see cref="PauseSessionCommand"/> instance.</returns>
    public static PauseSessionCommand CreateNew(string sessionId = "")
    {
        ArgumentNullException.ThrowIfNull(sessionId);

        return new PauseSessionCommand(
            id: Guid.NewGuid(),
            correlationId: Guid.NewGuid(),
            timestamp: DateTimeOffset.UtcNow,
            sessionId);
    }
}