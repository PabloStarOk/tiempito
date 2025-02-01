using Tiempitod.NET.Common;
using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands.SessionManagement;

/// <summary>
/// Represents a command to pause a session.
/// </summary>
/// <param name="sessionService">The session manager to handle the session operations.</param>
/// <param name="arguments">The arguments containing the session ID.</param>
public readonly struct PauseSessionCommand(
    ISessionService sessionService,
    IReadOnlyDictionary<string, string> arguments) 
    : ICommand
{
    public Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
   {
      arguments.TryGetValue("session-id", out string? sessionId);
      return Task.FromResult(sessionService.PauseSession(sessionId ?? string.Empty));
   }
}
