using Tiempito.IPC.NET.Messages;
using Tiempitod.NET.Commands.SessionCommands;
using Tiempitod.NET.Configuration.Session;
using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands;

/// <summary>
/// Handles command requests related to sessions.
/// </summary>
public class SessionCommandHandler : ICommandHandler
{
    private readonly ISessionConfigProvider _sessionConfigProvider;
    private readonly ISessionManager _sessionManager;
    
    /// <summary>
    /// Instantiates a <see cref="SessionCommandHandler"/>.
    /// </summary>
    /// <param name="sessionConfigProvider">Provider of session configurations.</param>
    /// <param name="sessionManager">Manager of sessions.</param>
    public SessionCommandHandler(
        ISessionConfigProvider sessionConfigProvider,
        ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;
        _sessionConfigProvider = sessionConfigProvider;
    }
    
    public async Task<OperationResult> HandleCommandAsync(Request request, CancellationToken cancellationToken = default)
    {
        if (!TryCreateCommand(request, out ICommand? command))
        {
            return new OperationResult
            (
                Success: false,
                Message: $"Unknown command '{request.SubcommandType}'"
            );
        }
        return await command.ExecuteAsync(cancellationToken);
    }

    /// <summary>
    /// Tries to create command with the given command request.
    /// </summary>
    /// <param name="request">Request to create the command.</param>
    /// <param name="command">Created command.</param>
    /// <returns>True if the command was created, false otherwise.</returns>
    private bool TryCreateCommand(Request request, out ICommand? command)
    {
        command = null;
        IDictionary<string, string> args = request.Arguments;
        
        command = request.SubcommandType switch
        {
            "start" => new StartSessionCommand(_sessionManager, args),
            "pause" => new PauseSessionCommand(_sessionManager, args),
            "resume" => new ResumeSessionCommand(_sessionManager, args),
            "cancel" => new CancelSessionCommand(_sessionManager, args),
            "create" => new CreateSessionCommand(_sessionConfigProvider, args),
            _ => null
        };

        return command != null;
    }
}
