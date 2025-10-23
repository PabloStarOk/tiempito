using System.CommandLine;

using Tiempito.CLI.Client.Interfaces;

namespace Tiempito.CLI.Session;

/// <summary>
/// Represents a generic session command that uses only the session id as argument.
/// </summary>
public class GenericSessionCommand : Command
{
    private readonly IAsyncCommandExecutor _asyncCommandExecutor;
    private readonly string _commandParent;
    
    /// <summary>
    /// Instantiates a <see cref="GenericSessionCommand"/>.
    /// </summary>
    /// <param name="asyncCommandExecutor">An asynchronous executor of commands.</param>
    /// <param name="commandParent">Command parent of this command.</param>
    /// <param name="sessionIdOption">Session id option.</param>
    /// <param name="name">Name of the command.</param>
    /// <param name="description">Description of the command.</param>
    /// <param name="interactiveOption">Optional interactive option to keep connection with server.</param>
    public GenericSessionCommand(
        IAsyncCommandExecutor asyncCommandExecutor,
        string commandParent, Option<string> sessionIdOption,
        string name, string description, Option<bool>? interactiveOption = null) : base(name, description)
    {
        _asyncCommandExecutor = asyncCommandExecutor;
        _commandParent = commandParent;
        
        sessionIdOption.IsRequired = false;
        AddOption(sessionIdOption);
        
        if (interactiveOption != null)
        {
            AddOption(interactiveOption);
            this.SetHandler(CommandHandler, sessionIdOption, interactiveOption);
            return;
        }
        this.SetHandler(CommandHandler, sessionIdOption);
    }
    
    /// <summary>
    /// Sends the request to manage a tiempito session.
    /// </summary>
    /// <param name="sessionId">ID of the session to use.</param>
    private async Task CommandHandler(string sessionId)
    {
        var arguments = new Dictionary<string, string>
        {
            { "session-id", sessionId }
        };
        await _asyncCommandExecutor.ExecuteAsync(_commandParent, subcommand: Name, arguments);
    }
    
    /// <summary>
    /// Sends the request to manage a tiempito session.
    /// </summary>
    /// <param name="sessionId">ID of the session to use.</param>
    /// <param name="interactiveOption">If the session's progress is redirected to the current process.</param>
    private async Task CommandHandler(string sessionId, bool interactiveOption)
    {
        var arguments = new Dictionary<string, string>
        {
            { "session-id", sessionId }
        };
        await _asyncCommandExecutor.ExecuteAsync(_commandParent, subcommand: Name, arguments, interactiveOption);
    }
}
