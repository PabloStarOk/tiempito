using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands.Session;

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
   
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await _sessionManager.CancelSessionAsync();
    }
}
