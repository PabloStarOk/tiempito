using System.CommandLine;

using Tiempito.CLI.Client.Interfaces;

namespace Tiempito.CLI.Commands.Config;

/// <summary>
/// Represents the command to create a new session configuration.
/// </summary>
public class CreateSessionConfigCommand : Command
{
    private const string CommandName = "create-session-config";
    private const string CommandDescription = "Creates a new session configuration.";

    private readonly IAsyncCommandExecutor _asyncCommandExecutor;
    private readonly string _commandParent;
    private readonly Option<string> _sessionIdOption;
    private readonly Option<string> _targetCyclesOption;
    private readonly Option<string> _delayOption;
    private readonly Option<string> _focusDurationOption;
    private readonly Option<string> _breakDurationOption;

    /// <summary>
    /// Instantiates a <see cref="CreateSessionConfigCommand"/>.
    /// </summary>
    /// <param name="asyncCommandExecutor">An asynchronous executor of commands.</param>
    /// <param name="commandParent">Command parent of this command.</param>
    /// <param name="sessionIdOption">ID option of the new session configuration.</param>
    public CreateSessionConfigCommand(
        IAsyncCommandExecutor asyncCommandExecutor,
        string commandParent, Option<string> sessionIdOption) 
        : base(CommandName, CommandDescription)
    {
        _asyncCommandExecutor = asyncCommandExecutor;
        _commandParent = commandParent;
        Aliases.Add("create-session");
        _sessionIdOption = sessionIdOption;
        _sessionIdOption.Required = true;

        _targetCyclesOption = new Option<string>("--target-cycles", "-t")
        {
            Description = "Target cycles to complete.",
            Arity = ArgumentArity.ExactlyOne,
            Required = true
        };

        _delayOption = new Option<string>("--delay-between-times", "-d")
        {
            Description = "Delay before starting a time after another has been completed.",
            Arity = ArgumentArity.ExactlyOne,
            Required = false
        };

        _focusDurationOption = new Option<string>("--focus-duration", "-f")
        {
            Description = "The duration of a focus time.",
            Arity = ArgumentArity.ExactlyOne,
            Required = true
        };

        _breakDurationOption = new Option<string>("--break-duration", "-b")
        {
            Description = "The duration of a break time.",
            Arity = ArgumentArity.ExactlyOne,
            Required = true
        };

        Add(_sessionIdOption);
        Add(_targetCyclesOption);
        Add(_delayOption);
        Add(_focusDurationOption);
        Add(_breakDurationOption);
        SetAction(ExecuteAsync);
    }

    private async Task ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var delayBetweenTimes = parseResult.GetValue(_delayOption);
        delayBetweenTimes = !string.IsNullOrWhiteSpace(delayBetweenTimes) ? delayBetweenTimes : "0s";

        var arguments = new Dictionary<string, string>
        {
            { "session-config-id", parseResult.GetRequiredValue(_sessionIdOption) },
            { "target-cycles", parseResult.GetRequiredValue(_targetCyclesOption) },
            { "delay-times", delayBetweenTimes },
            { "focus-duration", parseResult.GetRequiredValue(_focusDurationOption) },
            { "break-duration", parseResult.GetRequiredValue(_breakDurationOption) },
        };
        await _asyncCommandExecutor.ExecuteAsync(_commandParent, subcommand: Name, arguments);
    }
}
