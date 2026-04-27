using MessagePack;

using Tiempito.IPC.Models.Commands.Config;
using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.IPC.Models;

/// <summary>
/// Base message used for inter-process communication.
/// </summary>
/// <param name="Id">Unique identifier for this message.</param>
/// <param name="CorrelationId">Identifier used to correlate related messages.</param>
/// <param name="Timestamp">Timestamp when the message was created (offset-aware).</param>
[MessagePackObject]
[Union(0, typeof(Response))]
[Union(1, typeof(SessionProgressMessage))]
[Union(2, typeof(ConnectionTerminationMessage))]
[Union(3, typeof(StartSessionCommand))]
[Union(4, typeof(PauseSessionCommand))]
[Union(5, typeof(ResumeSessionCommand))]
[Union(6, typeof(CancelSessionCommand))]
[Union(7, typeof(CreateSessionConfigCommand))]
[Union(8, typeof(ModifySessionConfigCommand))]
[Union(9, typeof(SetConfigCommand))]
[Union(10, typeof(UserFeatureConfigCommand))]
public abstract record Message(
    [property: Key(0)] Guid Id,
    [property: Key(1)] Guid CorrelationId,
    [property: Key(2)] DateTimeOffset Timestamp);