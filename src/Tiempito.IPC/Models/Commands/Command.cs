using MessagePack;

using Tiempito.IPC.Models.Commands.Config;
using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.IPC.Models.Commands;

/// <summary>
/// Base abstract command record used for IPC messaging.
/// </summary>
/// <param name="Id">Unique identifier for this command.</param>
/// <param name="CorrelationId">Identifier used to correlate related messages or requests.</param>
/// <param name="Timestamp">Timestamp indicating when the command was created.</param>
[MessagePackObject]
[Union(0, typeof(StartSessionCommand))]
[Union(1, typeof(PauseSessionCommand))]
[Union(2, typeof(ResumeSessionCommand))]
[Union(3, typeof(CancelSessionCommand))]
[Union(4, typeof(CreateSessionConfigCommand))]
[Union(5, typeof(ModifySessionConfigCommand))]
[Union(6, typeof(SetConfigCommand))]
[Union(7, typeof(UserFeatureConfigCommand))]
public abstract record Command(
    Guid Id,
    Guid CorrelationId,
    DateTimeOffset Timestamp)
    : Message(Id, CorrelationId, Timestamp);
