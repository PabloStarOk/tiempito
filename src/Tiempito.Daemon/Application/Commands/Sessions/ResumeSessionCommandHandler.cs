using Tiempito.Daemon.Application.Sessions;
using Tiempito.Daemon.Domain.Commands.Enums;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models;

namespace Tiempito.Daemon.Application.Commands.Sessions;

/// <summary>
/// Handles the command to resume a session using the provided <see cref="ISessionService"/>.
/// </summary>
internal sealed class ResumeSessionCommandHandler(ISessionService sessionService)
    : ICommandHandler
{
    /// <inheritdoc/>
    public bool CanHandle(Command command) =>
        command.CommandType.Equals(nameof(CommandType.Session), StringComparison.OrdinalIgnoreCase)
        && command.SubcommandType == "resume";

    /// <inheritdoc/>
    public ValueTask<OperationResult> HandleAsync(Command command, CancellationToken cancellationToken = default)
    {
        command.Arguments.TryGetValue("session-id", out string? sessionId);
        var result = sessionService.ResumeSession(sessionId ?? string.Empty);
        return ValueTask.FromResult(result);
    }
}