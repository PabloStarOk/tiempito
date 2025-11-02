using MessagePack;

namespace Tiempito.IPC.Models;

/// <summary>
/// Represents a message indicating the termination of a connection.
/// </summary>
[MessagePackObject(AllowPrivate = true)]
public sealed record ConnectionTerminationMessage : Message
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionTerminationMessage"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the message.</param>
    /// <param name="correlationId">The correlation identifier to associate related messages.</param>
    /// <param name="timestamp">The UTC timestamp when the message was created.</param>
    [SerializationConstructor]
    internal ConnectionTerminationMessage(Guid id, Guid correlationId, DateTimeOffset timestamp)
        : base(id, correlationId, timestamp)
    {
    }

    /// <summary>
    /// Creates a new <see cref="ConnectionTerminationMessage"/> instance with a new unique ID,
    /// the specified correlation ID (or a new one if not provided), and the current UTC timestamp.
    /// </summary>
    /// <returns>A new <see cref="ConnectionTerminationMessage"/> instance.</returns>
    public static ConnectionTerminationMessage CreateNew()
    {
        return new ConnectionTerminationMessage(
            id: Guid.NewGuid(),
            correlationId: Guid.NewGuid(),
            DateTimeOffset.UtcNow);
    }
}