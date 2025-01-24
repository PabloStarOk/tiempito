using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands.SessionCommands;

/// <summary>
/// Command that continues a paused session.
/// </summary>
public class ResumeSessionCommand : ICommand
{
    private readonly ISessionManager _sessionManager;
    private readonly IReadOnlyDictionary<string, string> _arguments;
   
    public ResumeSessionCommand(ISessionManager sessionManager, IReadOnlyDictionary<string, string> arguments)
    {
        _sessionManager = sessionManager;
        _arguments = arguments;
    }
   
    public Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _arguments.TryGetValue("session-id", out string? sessionId);
        return Task.FromResult(_sessionManager.ResumeSession(sessionId ?? string.Empty));
    }
}
