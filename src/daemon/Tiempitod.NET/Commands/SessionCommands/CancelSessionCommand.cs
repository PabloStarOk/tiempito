using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands.SessionCommands;

/// <summary>
/// Command that cancels the current active session.
/// </summary>
public class CancelSessionCommand : ICommand
{
    private readonly ISessionManager _sessionManager;
   
    public CancelSessionCommand(ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }
   
    public async Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return await _sessionManager.CancelSessionAsync();
    }
}
