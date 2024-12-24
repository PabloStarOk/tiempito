using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands.Session;

/// <summary>
/// Command that continues a paused session.
/// </summary>
public class ResumeSessionCommand : ICommand
{
    private readonly ISessionManager _sessionManager;
   
    public ResumeSessionCommand(ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }
   
    public Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_sessionManager.ResumeSession());
    }
}
