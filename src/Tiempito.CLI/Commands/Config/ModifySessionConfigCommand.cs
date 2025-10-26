using System.CommandLine;

using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Models;

using Command = System.CommandLine.Command;

namespace Tiempito.CLI.Commands.Config;

/// <summary>
/// Represents the command to modify an existing session configuration.
/// </summary>
public class ModifySessionConfigCommand : Command
{
    private const string CommandName = "modify-session";
    private const string CommandDescription = "Modifies an existing session configuration.";

    private readonly ICommandSender _commandSender;
    private readonly IMessageWriter _messageWriter;
    private readonly string _commandParent;
    private readonly Option<string> _sessionIdOption;
    private readonly Option<string> _targetCyclesOption;
    private readonly Option<string> _delayOption;
    private readonly Option<string> _focusDurationOption;
    private readonly Option<string> _breakDurationOption;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModifySessionConfigCommand"/> class.
    /// </summary>
    /// <param name="commandSender">The sender used to execute session commands.</param>
    /// <param name="messageWriter">The writer used to output messages to the terminal.</param>
    /// <param name="commandParent">Command parent of this command.</param>
    /// <param name="sessionIdOption">Session id option.</param>
    public ModifySessionConfigCommand(
        ICommandSender commandSender,
        IMessageWriter messageWriter,
        string commandParent,
        Option<string> sessionIdOption)
        : base(CommandName, CommandDescription)
    {
        _commandSender = commandSender;
        _messageWriter = messageWriter;
        _commandParent = commandParent;
        _sessionIdOption = sessionIdOption;
        _sessionIdOption.Required = true;

        _targetCyclesOption = new Option<string>("--target-cycles", "-t")
        {
            Description = "Target cycles to complete.",
            Arity = ArgumentArity.ExactlyOne,
            Required = false,
        };

        _delayOption = new Option<string>("--delay-between-times", "-d")
        {
            Description = "Delay before starting a time after another has been completed.",
            Arity = ArgumentArity.ExactlyOne,
            Required = false,
        };

        _focusDurationOption = new Option<string>("--focus-duration", "-f")
        {
            Description = "The duration of a focus time.",
            Arity = ArgumentArity.ExactlyOne,
            Required = false,
        };

        _breakDurationOption = new Option<string>("--break-duration", "-b")
        {
            Description = "The duration of a break time.",
            Arity = ArgumentArity.ExactlyOne,
            Required = false,
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
            { "target-cycles", parseResult.GetValue(_targetCyclesOption) ?? string.Empty },
            { "delay-times", delayBetweenTimes },
            { "focus-duration", parseResult.GetValue(_focusDurationOption) ?? string.Empty },
            { "break-duration", parseResult.GetValue(_breakDurationOption) ?? string.Empty },
        };

        Response? response = await _commandSender.SendAsync(
            _commandParent,
            Name,
            arguments,
            cancellationToken: cancellationToken);

        if (response is not null)
        {
            await _messageWriter.WriteAsync(error: !response.Success, response.Message, cancellationToken);
        }
    }
}
