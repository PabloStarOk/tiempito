using Tiempito.IPC.NET.Messages;
using Tiempitod.NET.Commands.Configuration;
using Tiempitod.NET.Configuration.Session;
using Tiempitod.NET.Exceptions;
using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands.SessionManagement;

// TODO: This class can be refactored along with ConfigCommandsHandler to provide only the CreateCommand method.
/// <summary>
/// Handles command requests related to sessions.
/// </summary>
public class SessionCommandsHandler : ICommandHandler
{
    private readonly ISessionConfigProvider _sessionConfigProvider;
    private readonly ISessionManager _sessionManager;
    
    /// <summary>
    /// Instantiates a <see cref="SessionCommandsHandler"/>.
    /// </summary>
    /// <param name="sessionConfigProvider">Provider of session configurations.</param>
    /// <param name="sessionManager">Manager of sessions.</param>
    public SessionCommandsHandler(
        ISessionConfigProvider sessionConfigProvider,
        ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;
        _sessionConfigProvider = sessionConfigProvider;
    }
    
    public async Task<OperationResult> HandleCommandAsync(Request request, CancellationToken cancellationToken = default)
    {
        try
        {
            ICommand command = CreateCommand(request);
            return await command.ExecuteAsync(cancellationToken);
        }
        catch (CommandNotFoundException ex)
        {
            return new OperationResult
            (
                Success: false,
                Message: ex.Message
            );
        }
    }

    /// <summary>
    /// Creates a command with the given command request.
    /// </summary>
    /// <param name="request">Request to create the command.</param>
    /// <returns>An <see cref="ICommand"/>.</returns>
    /// <exception cref="CommandNotFoundException">If the given command is not recognized.</exception>>
    private ICommand CreateCommand(Request request)
    {
        IReadOnlyDictionary<string, string> args = request.Arguments;
        
        return request.SubcommandType switch
        {
            "start" => new StartSessionCommand(_sessionManager, args),
            "pause" => new PauseSessionCommand(_sessionManager, args),
            "resume" => new ResumeSessionCommand(_sessionManager, args),
            "cancel" => new CancelSessionCommand(_sessionManager, args),
            "create" => new CreateSessionConfigCommand(_sessionConfigProvider, args),
            "modify" => new ModifySessionConfigCommand(_sessionConfigProvider, args),
            _ => throw new CommandNotFoundException(request.SubcommandType)
        };
    }
}
