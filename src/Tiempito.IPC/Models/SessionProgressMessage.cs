using MessagePack;

using Tiempito.IPC.Models.Enums;

namespace Tiempito.IPC.Models;

/// <summary>
/// Represents a message containing progress information for a session.
/// </summary>
[MessagePackObject(AllowPrivate = true)]
public sealed record SessionProgressMessage : Message
{
    /// <summary>
    /// Gets the unique identifier for the session.
    /// </summary>
    [Key(3)]
    public string SessionId { get; }

    /// <summary>
    /// Gets the duration of the current interval.
    /// </summary>
    [Key(4)]
    public TimeSpan IntervalDuration { get; }

    /// <summary>
    /// Gets the type of the current session interval.
    /// </summary>
    [Key(5)]
    public SessionIntervalType IntervalType { get; }

    /// <summary>
    /// Gets the current cycle number within the session.
    /// </summary>
    [Key(6)]
    public int Cycle { get; }

    /// <summary>
    /// Gets the elapsed time for the current interval.
    /// </summary>
    [Key(7)]
    public TimeSpan ElapsedTime { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionProgressMessage"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the message.</param>
    /// <param name="correlationId">The correlation identifier for tracking related messages.</param>
    /// <param name="timestamp">The timestamp when the message was created.</param>
    /// <param name="sessionId">The unique identifier for the session.</param>
    /// <param name="intervalDuration">The duration of the current interval.</param>
    /// <param name="intervalType">The type of the current session interval.</param>
    /// <param name="cycle">The current cycle number within the session.</param>
    /// <param name="elapsedTime">The elapsed time for the current interval.</param>
    internal SessionProgressMessage(
        Guid id,
        Guid correlationId,
        DateTimeOffset timestamp,
        string sessionId,
        TimeSpan intervalDuration,
        SessionIntervalType intervalType,
        int cycle,
        TimeSpan elapsedTime)
        : base(id, correlationId, timestamp)
    {
        SessionId = sessionId;
        IntervalDuration = intervalDuration;
        IntervalType = intervalType;
        Cycle = cycle;
        ElapsedTime = elapsedTime;
    }

    /// <summary>
    /// Creates a new <see cref="SessionProgressMessage"/> instance with a new unique identifier and correlation ID.
    /// </summary>
    /// <param name="sessionId">The unique identifier for the session.</param>
    /// <param name="intervalDuration">The duration of the current interval.</param>
    /// <param name="intervalType">The type of the current session interval.</param>
    /// <param name="cycle">The current cycle number within the session.</param>
    /// <param name="elapsedTime">The elapsed time for the current interval.</param>
    /// <returns>A new <see cref="SessionProgressMessage"/> instance.</returns>
    public static SessionProgressMessage CreateNew(
        string sessionId,
        TimeSpan intervalDuration,
        SessionIntervalType intervalType,
        int cycle,
        TimeSpan elapsedTime)
    {
        return new SessionProgressMessage(
            id: Guid.NewGuid(),
            correlationId: Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            sessionId,
            intervalDuration,
            intervalType,
            cycle,
            elapsedTime);
    }
}