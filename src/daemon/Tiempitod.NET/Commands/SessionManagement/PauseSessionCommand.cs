using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands.SessionManagement;

/// <summary>
/// Command that pauses the current executing session.
/// </summary>
public class PauseSessionCommand : ICommand
{
   private readonly ISessionManager _sessionManager;
   private readonly IReadOnlyDictionary<string, string> _arguments;
   
   public PauseSessionCommand(ISessionManager sessionManager, IReadOnlyDictionary<string, string> arguments)
   {
      _sessionManager = sessionManager;
      _arguments = arguments;
   }
   
   public Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
   {
      _arguments.TryGetValue("session-id", out string? sessionId);
      return Task.FromResult(_sessionManager.PauseSession(sessionId ?? string.Empty));
   }
}
