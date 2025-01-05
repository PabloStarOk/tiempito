using System.CommandLine;
using Tiempito.CLI.NET.Client;

namespace Tiempito.CLI.NET.Session;

/// <summary>
/// 
/// </summary>
public class SessionCommand
{
    private readonly IAsyncCommandExecutor _asyncCommandExecutor;
    private readonly Option<string> _sessionIdOption;
    
    /// <summary>
    /// Instantiates a <see cref="SessionCommand"/>
    /// </summary>
    /// <param name="asyncCommandExecutor">An asynchronous executor of commands.</param>
    public SessionCommand(IAsyncCommandExecutor asyncCommandExecutor)
    {
        _asyncCommandExecutor = asyncCommandExecutor;
        _sessionIdOption = new Option<string>("--id", "ID of the session configuration.")
        {
            Arity = ArgumentArity.ExactlyOne
        };
        _sessionIdOption.AddAlias("-i");
    }

    /// <summary>
    /// Creates a new <see cref="Command"/> named session with grouping
    /// all subcommands to manage a session.
    /// </summary>
    /// <returns>A configured <see cref="Command"/>.</returns>
    public Command GetCommand()
    {
        var sessionCommand = new Command(name: "session", description: "Manage sessions.");

        sessionCommand.AddCommand(new CreateSessionCommand(_asyncCommandExecutor, sessionCommand.Name, _sessionIdOption));
        sessionCommand.AddCommand(new ModifySessionCommand(_asyncCommandExecutor, sessionCommand.Name, _sessionIdOption));
        
        sessionCommand.AddCommand(new GenericSessionCommand(
            _asyncCommandExecutor, sessionCommand.Name, _sessionIdOption,
            "start", "Starts a new session."));
        
        sessionCommand.AddCommand(new GenericSessionCommand(
            _asyncCommandExecutor, sessionCommand.Name, _sessionIdOption,
            "cancel", "Cancels a session that is being executed."));
        
        sessionCommand.AddCommand(new GenericSessionCommand(
            _asyncCommandExecutor, sessionCommand.Name, _sessionIdOption,
            "pause", "Pauses a session that is being executed."));
        
        sessionCommand.AddCommand(new GenericSessionCommand(
            _asyncCommandExecutor, sessionCommand.Name, _sessionIdOption,
            "resume", "Resumes a paused session."));
        
        return sessionCommand;
    }
}
