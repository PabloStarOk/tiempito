using System.CommandLine;
using Tiempito.CLI.NET.Client;

namespace Tiempito.CLI.NET.Session;

public class SessionCommand
{
    private readonly IClient _client;
    private readonly Option<string> _sessionIdOption;
    
    public SessionCommand(IClient client)
    {
        _client = client;
        _sessionIdOption = new Option<string>("--id", "ID of the session configuration.")
        {
            Arity = ArgumentArity.ExactlyOne
        };
        _sessionIdOption.AddAlias("-i");
    }

    public Command GetCommand()
    {
        var sessionCommand = new Command(name: "session", description: "Manage sessions.");

        sessionCommand.AddCommand(new CreateCommand(_client, _sessionIdOption));
        sessionCommand.AddCommand(new ModifyCommand(_client, _sessionIdOption));
        sessionCommand.AddCommand(BuildStartCommand());
        sessionCommand.AddCommand(BuildCancelCommand());
        sessionCommand.AddCommand(BuildPauseCommand());
        sessionCommand.AddCommand(BuildResumeCommand());
        
        return sessionCommand;
    }
    
    private StartCommand BuildStartCommand()
    {
        return new StartCommand(
            _client, _sessionIdOption, 
            name: "start", description: "Starts a new session.");
    }
    
    private CancelCommand BuildCancelCommand()
    {
        return new CancelCommand(
            _client, _sessionIdOption, 
            name: "cancel", description: "Cancels a session that is being executed.");
    }
    
    private PauseCommand BuildPauseCommand()
    {
        return new PauseCommand(
            _client, _sessionIdOption, 
            name: "pause", description: "Pauses a session that is being executed.");
    }
    
    private ResumeCommand BuildResumeCommand()
    {
        return new ResumeCommand(
            _client, _sessionIdOption, 
            name: "resume", description: "Resumes a paused session.");
    }
}
