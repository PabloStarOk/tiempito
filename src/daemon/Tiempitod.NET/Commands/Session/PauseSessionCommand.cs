using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands.Session;

/// <summary>
/// Command that pauses the current executing session.
/// </summary>
public class PauseSessionCommand : ICommand
{
   private readonly ISessionManager _sessionManager;
   
   public PauseSessionCommand(ISessionManager sessionManager)
   {
      _sessionManager = sessionManager;
   }
   
   public async Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
   {
      return await _sessionManager.PauseSessionAsync();
   }
}
