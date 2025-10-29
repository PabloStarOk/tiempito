using Tiempito.Daemon.Application.Sessions;
using Tiempito.Daemon.Domain.Commands.Enums;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models;

namespace Tiempito.Daemon.Application.Commands.Sessions;

/// <summary>
/// Handles the cancellation of sessions by invoking the <see cref="ISessionService"/>.
/// </summary>
/// <param name="sessionService">Service used to manage sessions.</param>
internal sealed class CancelSessionCommandHandler(ISessionService sessionService)
    : ICommandHandler
{
    /// <inheritdoc/>
    public bool CanHandle(Command command) =>
        command.CommandType.Equals(nameof(CommandType.Session), StringComparison.OrdinalIgnoreCase)
        && command.SubcommandType == "cancel";

    /// <inheritdoc/>
    public async ValueTask<OperationResult> HandleAsync(Command command, CancellationToken cancellationToken = default)
    {
        command.Arguments.TryGetValue("session-id", out string? sessionId);
        var result = await sessionService.CancelSessionAsync(sessionId ?? string.Empty);
        return result;
    }
}
