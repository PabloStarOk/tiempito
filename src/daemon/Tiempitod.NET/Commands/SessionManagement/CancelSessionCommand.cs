using Tiempitod.NET.Common;
using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands.SessionManagement;

/// <summary>
/// Represents a command to cancel a session.
/// </summary>
/// <param name="sessionManager">The session manager to handle the session cancellation.</param>
/// <param name="arguments">The arguments containing the session ID.</param>
public readonly struct CancelSessionCommand(
    ISessionManager sessionManager,
    IReadOnlyDictionary<string, string> arguments)
    : ICommand
{
    public Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        arguments.TryGetValue("session-id", out string? sessionId);
        return Task.FromResult(sessionManager.CancelSession(sessionId ?? string.Empty));
    }
}
