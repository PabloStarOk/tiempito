using System.CommandLine;
using Tiempito.CLI.NET.Client;

namespace Tiempito.CLI.NET.Session;

/// <summary>
/// Starts a new session.
/// </summary>
public class StartSessionCommand : Command
{
    private readonly IAsyncCommandExecutor _asyncCommandExecutor;
    private readonly string _commandParent;
    
    /// <summary>
    /// Instantiates a <see cref="StartSessionCommand"/>.
    /// </summary>
    /// <param name="asyncCommandExecutor">An asynchronous executor of commands.</param>
    /// <param name="commandParent">Command parent of this command.</param>
    /// <param name="sessionIdOption">Session id option.</param>
    /// <param name="name">Name of the command.</param>
    /// <param name="description">Description of the command.</param>
    public StartSessionCommand(
        IAsyncCommandExecutor asyncCommandExecutor,
        string commandParent, Option<string> sessionIdOption,
        string name, string description) : base(name, description)
    {
        _asyncCommandExecutor = asyncCommandExecutor;
        _commandParent = commandParent;
        
        sessionIdOption.IsRequired = false;
        

        var sessionConfigIdOption = new Option<string>("--config-id", "ID of the session configuration.")
        {
            Arity = ArgumentArity.ZeroOrOne
        };
        sessionConfigIdOption.AddAlias("-ci");
        
        var interactiveOption = new Option<bool>("--tty", "Redirects the progress of the session to the current process.")
        {
            Arity = ArgumentArity.ZeroOrOne,
            IsRequired = false
        };
        interactiveOption.AddAlias("-t");
        
        AddOption(sessionIdOption);
        AddOption(sessionConfigIdOption);
        AddOption(interactiveOption);
        this.SetHandler(CommandHandler, sessionIdOption, sessionConfigIdOption, interactiveOption);
    }
    
    /// <summary>
    /// Sends the request to manage a tiempito session.
    /// </summary>
    /// <param name="sessionId">ID of the session to use.</param>
    /// <param name="sessionConfigId">ID of the config to use for the new session.</param>
    /// <param name="tty">If the session's progress is redirected to the current process.</param>
    private async Task CommandHandler(string sessionId, string sessionConfigId, bool tty)
    {
        var arguments = new Dictionary<string, string>
        {
            { "session-id", sessionId },
            { "session-config-id", sessionConfigId }
        };
        await _asyncCommandExecutor.ExecuteAsync(_commandParent, subcommand: Name, arguments, tty);
    }
}
