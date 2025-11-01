using System.CommandLine;

using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Models;

using Command = System.CommandLine.Command;
using IpcModifySessionConfigCommand = Tiempito.IPC.Models.Commands.Config.ModifySessionConfigCommand;

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
    /// <param name="sessionIdOption">Session id option.</param>
    public ModifySessionConfigCommand(
        ICommandSender commandSender,
        IMessageWriter messageWriter,
        Option<string> sessionIdOption)
        : base(CommandName, CommandDescription)
    {
        _commandSender = commandSender;
        _messageWriter = messageWriter;
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
        string sessionConfigId = parseResult.GetRequiredValue(_sessionIdOption);
        string? targetCyclesString = parseResult.GetValue(_targetCyclesOption);
        uint targetCycles = 0;
        if (targetCyclesString is not null && !uint.TryParse(targetCyclesString, out targetCycles))
        {
            await _messageWriter.WriteAsync(
                error: true,
                message: "Target cycles must be a valid positive number.",
                cancellationToken);
            return;
        }

        string? focusDuration = parseResult.GetValue(_focusDurationOption);
        string? breakDuration = parseResult.GetValue(_breakDurationOption);
        string? delayBetweenTimes = parseResult.GetValue(_delayOption);

        var command = IpcModifySessionConfigCommand.CreateNew(
            sessionConfigId,
            targetCycles,
            focusDuration,
            breakDuration,
            delayBetweenTimes);

        Response? response = await _commandSender.SendAsync(command, cancellationToken);

        if (response is not null)
        {
            await _messageWriter.WriteAsync(error: !response.Success, response.Message, cancellationToken);
        }
    }
}
