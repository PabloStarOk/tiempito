using Tiempito.Daemon.Common;
using Tiempito.Daemon.Sessions.Interfaces;

namespace Tiempito.Daemon.Commands.SessionManagement;

/// <summary>
/// Represents a command to resume a session.
/// </summary>
/// <param name="sessionService">The session manager responsible for handling session operations.</param>
/// <param name="arguments">The arguments containing the session ID.</param>
public readonly struct ResumeSessionCommand(
    ISessionService sessionService,
    IReadOnlyDictionary<string, string> arguments)
    : ICommand
{
    public Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        arguments.TryGetValue("session-id", out string? sessionId);
        return Task.FromResult(sessionService.ResumeSession(sessionId ?? string.Empty));
    }
}
