using System.CommandLine;
using Tiempito.CLI.NET.Client;

namespace Tiempito.CLI.NET.Session;

public class PauseCommand : Command
{
    private readonly IAsyncCommandExecutor _asyncCommandExecutor;
    private readonly string _commandParent;
    
    public PauseCommand(
        IAsyncCommandExecutor asyncCommandExecutor,
        string commandParent, Option<string> sessionIdOption,
        string name, string description) : base(name, description)
    {
        _asyncCommandExecutor = asyncCommandExecutor;
        _commandParent = commandParent;
        
        sessionIdOption.IsRequired = false;
        AddOption(sessionIdOption);
        this.SetHandler(CommandHandler, sessionIdOption);
    }
    
    private async Task CommandHandler(string sessionId)
    {
        var arguments = new Dictionary<string, string>
        {
            { "session-id", sessionId }
        };
        await _asyncCommandExecutor.ExecuteAsync(_commandParent, subcommand: Name, arguments);
    }
}
