using System.CommandLine;
using Tiempito.CLI.NET.Client;

namespace Tiempito.CLI.NET.Config;

/// <summary>
/// Represents the subcommand to modify parameters of the user's configuration.
/// </summary>
public class SetConfigCommand : Command
{
    private readonly IAsyncCommandExecutor _asyncCommandExecutor;
    private readonly string _commandParent;
    
    /// <summary>
    /// Instantiates a <see cref="SetConfigCommand"/>.
    /// </summary>
    /// <param name="asyncCommandExecutor">An asynchronous executor of commands.</param>
    /// <param name="commandParent">Command parent of this command.</param>
    /// <param name="defaultSessionIdOption">An option to change the default session of the user.</param>
    public SetConfigCommand(
        IAsyncCommandExecutor asyncCommandExecutor,
        string commandParent, Option<string> defaultSessionIdOption) 
        : base("set", "Sets the specified user configuration.")
    {
        _asyncCommandExecutor = asyncCommandExecutor;
        _commandParent = commandParent;
        defaultSessionIdOption.IsRequired = false;
        AddOption(defaultSessionIdOption);
        this.SetHandler(CommandHandler, defaultSessionIdOption);
    }
    
    /// <summary>
    /// Sends a request to the daemon to modify the provided arguments.
    /// </summary>
    /// <param name="defaultSessionId">New ID of the default session of the user.</param>
    private async Task CommandHandler(string defaultSessionId)
    {
        var arguments = new Dictionary<string, string>
        {
            { "default-session-id", defaultSessionId }
        };
        await _asyncCommandExecutor.ExecuteAsync(_commandParent, subcommand: Name, arguments);
    }
}
