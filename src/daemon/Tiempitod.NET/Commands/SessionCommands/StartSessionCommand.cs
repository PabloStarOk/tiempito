using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands.SessionCommands;

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
    
    public Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_sessionManager.StartSession(cancellationToken));
    }
}
