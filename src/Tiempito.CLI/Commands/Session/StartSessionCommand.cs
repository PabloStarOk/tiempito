using System.CommandLine;

using Tiempito.CLI.Client.Interfaces;

namespace Tiempito.CLI.Commands.Session;

/// <summary>
/// Starts a new session.
/// </summary>
public class StartSessionCommand : Command
{
    private const string CommandName = "start";
    private const string CommandDescription = "Starts a new session.";

    private readonly IAsyncCommandExecutor _asyncCommandExecutor;
    private readonly string _commandParent;
    private readonly Option<string> _sessionIdOption;
    private readonly Option<string> _sessionConfigIdOption;
    private readonly Option<bool>? _followOption;

    /// <summary>
    /// Instantiates a <see cref="StartSessionCommand"/>.
    /// </summary>
    /// <param name="asyncCommandExecutor">An asynchronous executor of commands.</param>
    /// <param name="commandParent">Command parent of this command.</param>
    /// <param name="sessionIdOption">Session id option.</param>
    /// <param name="followOption">Indicates whether to follow session progress.</param>
    public StartSessionCommand(
        IAsyncCommandExecutor asyncCommandExecutor,
        string commandParent, Option<string> sessionIdOption,
        Option<bool> followOption) : base(CommandName, CommandDescription)
    {
        _asyncCommandExecutor = asyncCommandExecutor;
        _commandParent = commandParent;
        _sessionIdOption = sessionIdOption;
        _followOption = followOption;

        _sessionIdOption.Required = false;
        _sessionConfigIdOption = new Option<string>("--config-id", "-ci")
        {
            Description = "ID of the session configuration.",
            Arity = ArgumentArity.ZeroOrOne,
        };

        Add(_sessionIdOption);
        Add(_sessionConfigIdOption);
        Add(_followOption);
        SetAction(ExecuteAsync);
    }

    private async Task ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var follow = _followOption is not null && parseResult.GetValue(_followOption);
        var arguments = new Dictionary<string, string>
        {
            { "session-id", parseResult.GetValue(_sessionIdOption) ?? string.Empty },
            { "session-config-id", parseResult.GetValue(_sessionConfigIdOption) ?? string.Empty },
        };
        await _asyncCommandExecutor.ExecuteAsync(_commandParent, subcommand: Name, arguments, follow);
    }
}
