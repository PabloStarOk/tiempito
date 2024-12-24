using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands.Session;

/// <summary>
/// Command that continues a paused session.
/// </summary>
public class ContinueSessionCommand : ICommand
{
    private readonly ISessionManager _sessionManager;
   
    public ContinueSessionCommand(ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }
   
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _sessionManager.ResumeSession();
        return Task.CompletedTask;
    }
}
