using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands.SessionManagement;

/// <summary>
/// Represents a command to resume a session.
/// </summary>
/// <param name="sessionManager">The session manager responsible for handling session operations.</param>
/// <param name="arguments">The arguments containing the session ID.</param>
public readonly struct ResumeSessionCommand(
    ISessionManager sessionManager,
    IReadOnlyDictionary<string, string> arguments)
    : ICommand
{
    public Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        arguments.TryGetValue("session-id", out string? sessionId);
        return Task.FromResult(sessionManager.ResumeSession(sessionId ?? string.Empty));
    }
}
