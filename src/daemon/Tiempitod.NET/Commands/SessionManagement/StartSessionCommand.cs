using Tiempitod.NET.Common;
using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands.SessionManagement;

/// <summary>
/// Represents command to start a new session.
/// </summary>
/// <param name="sessionManager">The session manager to handle session operations.</param>
/// <param name="arguments">The arguments containing the session ID and configuration ID.</param>
public readonly struct StartSessionCommand(
    ISessionManager sessionManager,
    IReadOnlyDictionary<string, string> arguments)
    : ICommand
{
    public Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        arguments.TryGetValue("session-id", out string? sessionId);
        arguments.TryGetValue("session-config-id", out string? sessionConfigId);
        return Task.FromResult(sessionManager.StartSession(sessionId ?? string.Empty, sessionConfigId ?? string.Empty));
    }
}
