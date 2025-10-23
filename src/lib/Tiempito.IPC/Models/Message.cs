using MessagePack;

namespace Tiempito.IPC.Models;

/// <summary>
/// Base message used for inter-process communication.
/// </summary>
/// <param name="Id">Unique identifier for this message.</param>
/// <param name="CorrelationId">Identifier used to correlate related messages.</param>
/// <param name="Timestamp">Timestamp when the message was created (offset-aware).</param>
[MessagePackObject]
[Union(0, typeof(Request))]
[Union(1, typeof(Response))]
public abstract record Message(
    [property: Key(0)] Guid Id,
    [property: Key(1)] Guid CorrelationId,
    [property: Key(2)] DateTimeOffset Timestamp);