using System.CommandLine;

using Tiempito.CLI.Client.Interfaces;

namespace Tiempito.CLI.Config;

/// <summary>
/// Represents the subcommand to modify parameters of the user's configuration.
/// </summary>
public class SetConfigCommand : Command
{
    private readonly IAsyncCommandExecutor _asyncCommandExecutor;
    private readonly string _commandParent;
    private readonly Option<string> _defaultSessionIdOption;

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
        _defaultSessionIdOption = defaultSessionIdOption;
        _defaultSessionIdOption.Required = false;
        Add(_defaultSessionIdOption);
        SetAction(ExecuteAsync);
    }

    private async Task ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var arguments = new Dictionary<string, string>
        {
            { "default-session-id", parseResult.GetRequiredValue(_defaultSessionIdOption) },
        };
        await _asyncCommandExecutor.ExecuteAsync(_commandParent, subcommand: Name, arguments);
    }
}
