using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands.SessionCommands;

/// <summary>
/// Command that cancels the current active session.
/// </summary>
public class CancelSessionCommand : ICommand
{
    private readonly ISessionManager _sessionManager;
    private readonly IDictionary<string, string> _arguments;
   
    public CancelSessionCommand(ISessionManager sessionManager, IDictionary<string, string> arguments)
    {
        _sessionManager = sessionManager;
        _arguments = arguments;
    }
   
    public async Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _arguments.TryGetValue("session-id", out string? sessionId);
        return await _sessionManager.CancelSessionAsync(sessionId ?? string.Empty);
    }
}
