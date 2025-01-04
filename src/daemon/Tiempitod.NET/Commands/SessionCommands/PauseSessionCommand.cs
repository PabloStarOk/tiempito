using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands.SessionCommands;

/// <summary>
/// Command that pauses the current executing session.
/// </summary>
public class PauseSessionCommand : ICommand
{
   private readonly ISessionManager _sessionManager;
   private readonly IDictionary<string, string> _arguments;
   
   public PauseSessionCommand(ISessionManager sessionManager, IDictionary<string, string> arguments)
   {
      _sessionManager = sessionManager;
      _arguments = arguments;
   }
   
   public async Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
   {
      _arguments.TryGetValue("session-id", out string? sessionId);
      return await _sessionManager.PauseSessionAsync(sessionId ?? string.Empty);
   }
}
