using Tiempito.Daemon.Application.Sessions;
using Tiempito.Daemon.Domain.Commands.Enums;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models;

namespace Tiempito.Daemon.Application.Commands.Sessions;

/// <summary>
/// Handles the start session command by invoking the session service.
/// </summary>
/// <param name="sessionService">Service responsible for session operations.</param>
internal sealed class StartSessionCommandHandler(ISessionService sessionService)
    : ICommandHandler
{
    /// <inheritdoc/>
    public bool CanHandle(Command command) =>
        command.CommandType.Equals(nameof(CommandType.Session), StringComparison.OrdinalIgnoreCase)
        && command.SubcommandType == "start";

    /// <inheritdoc/>
    public async ValueTask<OperationResult> HandleAsync(Command command, CancellationToken cancellationToken = default)
    {
        command.Arguments.TryGetValue("session-id", out string? sessionId);
        command.Arguments.TryGetValue("session-config-id", out string? sessionConfigId);
        var result = await sessionService.StartSessionAsync(sessionId ?? string.Empty, sessionConfigId ?? string.Empty);
        return result;
    }
}
