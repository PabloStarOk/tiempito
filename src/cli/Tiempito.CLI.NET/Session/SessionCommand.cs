using System.CommandLine;
using Tiempito.CLI.NET.Client;

namespace Tiempito.CLI.NET.Session;

public class SessionCommand
{
    private readonly IAsyncCommandExecutor _asyncCommandExecutor;
    private readonly Option<string> _sessionIdOption;
    
    public SessionCommand(IAsyncCommandExecutor asyncCommandExecutor)
    {
        _asyncCommandExecutor = asyncCommandExecutor;
        _sessionIdOption = new Option<string>("--id", "ID of the session configuration.")
        {
            Arity = ArgumentArity.ExactlyOne
        };
        _sessionIdOption.AddAlias("-i");
    }

    public Command GetCommand()
    {
        var sessionCommand = new Command(name: "session", description: "Manage sessions.");

        sessionCommand.AddCommand(new CreateCommand(_asyncCommandExecutor, sessionCommand.Name, _sessionIdOption));
        sessionCommand.AddCommand(new ModifyCommand(_asyncCommandExecutor, sessionCommand.Name, _sessionIdOption));
        sessionCommand.AddCommand(new StartCommand(
            _asyncCommandExecutor, sessionCommand.Name, _sessionIdOption, 
            name: "start", description: "Starts a new session."));
        sessionCommand.AddCommand(new CancelCommand(
            _asyncCommandExecutor, sessionCommand.Name, _sessionIdOption, 
            name: "cancel", description: "Cancels a session that is being executed."));
        sessionCommand.AddCommand(new PauseCommand(
            _asyncCommandExecutor, sessionCommand.Name, _sessionIdOption, 
            name: "pause", description: "Pauses a session that is being executed."));
        sessionCommand.AddCommand(new ResumeCommand(
            _asyncCommandExecutor, sessionCommand.Name, _sessionIdOption, 
            name: "resume", description: "Resumes a paused session."));
        
        return sessionCommand;
    }
}
