using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands.Session;

/// <summary>
/// Command that starts a session.
/// </summary>
public class StartSessionCommand : ICommand
{
    private readonly ISessionManager _sessionManager;
    
    public StartSessionCommand(ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }
    
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await _sessionManager.StartSessionAsync(cancellationToken);
    }
}
