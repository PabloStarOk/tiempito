using Tiempito.Daemon.Common;
using Tiempito.Daemon.Sessions.Interfaces;

namespace Tiempito.Daemon.Commands.SessionManagement;

/// <summary>
/// Represents command to start a new session.
/// </summary>
/// <param name="sessionService">The session manager to handle session operations.</param>
/// <param name="arguments">The arguments containing the session ID and configuration ID.</param>
public readonly struct StartSessionCommand(
    ISessionService sessionService,
    IReadOnlyDictionary<string, string> arguments)
    : ICommand
{
    public Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        arguments.TryGetValue("session-id", out string? sessionId);
        arguments.TryGetValue("session-config-id", out string? sessionConfigId);
        return Task.FromResult(sessionService.StartSession(sessionId ?? string.Empty, sessionConfigId ?? string.Empty));
    }
}
