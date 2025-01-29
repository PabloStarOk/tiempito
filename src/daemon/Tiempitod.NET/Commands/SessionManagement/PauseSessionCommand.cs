using Tiempitod.NET.Common;
using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands.SessionManagement;

/// <summary>
/// Represents a command to pause a session.
/// </summary>
/// <param name="sessionManager">The session manager to handle the session operations.</param>
/// <param name="arguments">The arguments containing the session ID.</param>
public readonly struct PauseSessionCommand(
    ISessionManager sessionManager,
    IReadOnlyDictionary<string, string> arguments) 
    : ICommand
{
    public Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
   {
      arguments.TryGetValue("session-id", out string? sessionId);
      return Task.FromResult(sessionManager.PauseSession(sessionId ?? string.Empty));
   }
}
