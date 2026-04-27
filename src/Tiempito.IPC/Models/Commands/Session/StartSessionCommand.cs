using MessagePack;

namespace Tiempito.IPC.Models.Commands.Session;

/// <summary>
/// Represents a command to start a session.
/// </summary>
[MessagePackObject(AllowPrivate = true)]
public sealed record StartSessionCommand : Command
{
    /// <summary>
    /// Gets the session identifier.
    /// </summary>
    [Key(3)]
    public string SessionId { get; }

    /// <summary>
    /// Gets the session configuration identifier.
    /// </summary>
    [Key(4)]
    public string SessionConfigId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StartSessionCommand"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the command.</param>
    /// <param name="correlationId">The correlation identifier for tracking related messages.</param>
    /// <param name="timestamp">The timestamp when the command was created.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="sessionConfigId">The session configuration identifier.</param>
    internal StartSessionCommand(
        Guid id,
        Guid correlationId,
        DateTimeOffset timestamp,
        string sessionId,
        string sessionConfigId)
        : base(id, correlationId, timestamp)
    {
        SessionId = sessionId;
        SessionConfigId = sessionConfigId;
    }

    /// <summary>
    /// Creates a new <see cref="StartSessionCommand"/> instance.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="sessionConfigId">The session configuration identifier.</param>
    /// <returns>A new <see cref="StartSessionCommand"/> instance.</returns>
    public static StartSessionCommand CreateNew(string sessionId = "", string sessionConfigId = "")
    {
        ArgumentNullException.ThrowIfNull(sessionId);
        ArgumentNullException.ThrowIfNull(sessionConfigId);

        return new StartSessionCommand(
            id: Guid.NewGuid(),
            correlationId: Guid.NewGuid(),
            timestamp: DateTimeOffset.UtcNow,
            sessionId,
            sessionConfigId);
    }
}