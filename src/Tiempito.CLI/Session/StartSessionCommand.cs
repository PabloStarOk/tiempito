using System.CommandLine;

using Tiempito.CLI.Client.Interfaces;

namespace Tiempito.CLI.Session;

/// <summary>
/// Starts a new session.
/// </summary>
public class StartSessionCommand : Command
{
    private readonly IAsyncCommandExecutor _asyncCommandExecutor;
    private readonly string _commandParent;
    private readonly Option<string> _sessionIdOption;
    private readonly Option<string> _sessionConfigIdOption;
    private readonly Option<bool>? _interactiveOption;

    /// <summary>
    /// Instantiates a <see cref="StartSessionCommand"/>.
    /// </summary>
    /// <param name="asyncCommandExecutor">An asynchronous executor of commands.</param>
    /// <param name="commandParent">Command parent of this command.</param>
    /// <param name="sessionIdOption">Session id option.</param>
    /// <param name="interactiveOption">If the session's progress is redirected to the current process.</param>
    /// <param name="name">Name of the command.</param>
    /// <param name="description">Description of the command.</param>
    public StartSessionCommand(
        IAsyncCommandExecutor asyncCommandExecutor,
        string commandParent, Option<string> sessionIdOption,
        Option<bool> interactiveOption, string name, string description) : base(name, description)
    {
        _asyncCommandExecutor = asyncCommandExecutor;
        _commandParent = commandParent;
        _sessionIdOption = sessionIdOption;
        _interactiveOption = interactiveOption;

        _sessionIdOption.Required = false;
        _sessionConfigIdOption = new Option<string>("--config-id", "-ci")
        {
            Description = "ID of the session configuration.",
            Arity = ArgumentArity.ZeroOrOne,
        };

        Add(_sessionIdOption);
        Add(_sessionConfigIdOption);
        Add(_interactiveOption);
        SetAction(ExecuteAsync);
    }

    private async Task ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var tty = _interactiveOption is not null && parseResult.GetValue(_interactiveOption);
        var arguments = new Dictionary<string, string>
        {
            { "session-id", parseResult.GetValue(_sessionIdOption) ?? string.Empty },
            { "session-config-id", parseResult.GetValue(_sessionConfigIdOption) ?? string.Empty },
        };
        await _asyncCommandExecutor.ExecuteAsync(_commandParent, subcommand: Name, arguments, tty);
    }
}
