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
    private readonly Option<string> _sessionIdOption;
    private readonly Option<bool>? _interactiveOption;

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
        _sessionIdOption = sessionIdOption;
        _interactiveOption = interactiveOption;

        _sessionIdOption.Required = false;
        Add(_sessionIdOption);

        if (_interactiveOption != null)
        {
            Add(_interactiveOption);
        }

        SetAction(ExecuteAsync);
    }

    private async Task ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var tty = _interactiveOption is not null && parseResult.GetValue(_interactiveOption);
        var arguments = new Dictionary<string, string>
        {
            { "session-id", parseResult.GetValue(_sessionIdOption) ?? string.Empty },
        };
        await _asyncCommandExecutor.ExecuteAsync(_commandParent, subcommand: Name, arguments, tty);
    }
}
