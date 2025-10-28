using Tiempito.Daemon.Application.Sessions;
using Tiempito.Daemon.Domain.Commands.Enums;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models;

namespace Tiempito.Daemon.Application.Commands.Sessions;

/// <summary>
/// Handles the pause session command by delegating to the <see cref="ISessionService"/>.
/// </summary>
/// <param name="sessionService">Service for session operations.</param>
internal sealed class PauseSessionCommandHandler(ISessionService sessionService)
    : ICommandHandler
{
    /// <inheritdoc/>
    public bool CanHandle(Command command) =>
        command.CommandType.Equals(nameof(CommandType.Session), StringComparison.OrdinalIgnoreCase)
        && command.SubcommandType == "pause";

    /// <inheritdoc/>
    public ValueTask<OperationResult> HandleAsync(Command command, CancellationToken cancellationToken = default)
    {
        command.Arguments.TryGetValue("session-id", out string? sessionId);
        var result = sessionService.PauseSession(sessionId ?? string.Empty);
        return ValueTask.FromResult(result);
    }
}
