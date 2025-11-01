using System.CommandLine;

using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Models;

using Command = System.CommandLine.Command;
using IpcCommand = Tiempito.IPC.Models.Commands.Config.CreateSessionConfigCommand;

namespace Tiempito.CLI.Commands.Config;

/// <summary>
/// Represents the command to create a new session configuration.
/// </summary>
public class CreateSessionConfigCommand : Command
{
    private const string CommandName = "create-session";
    private const string CommandDescription = "Creates a new session configuration.";

    private readonly ICommandSender _commandSender;
    private readonly IMessageWriter _messageWriter;
    private readonly Option<string> _sessionIdOption;
    private readonly Option<string> _targetCyclesOption;
    private readonly Option<string> _delayOption;
    private readonly Option<string> _focusDurationOption;
    private readonly Option<string> _breakDurationOption;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateSessionConfigCommand"/> class.
    /// </summary>
    /// <param name="commandSender">The sender used to execute session commands.</param>
    /// <param name="messageWriter">The writer used to output messages to the terminal.</param>
    /// <param name="sessionIdOption">ID option of the new session configuration.</param>
    public CreateSessionConfigCommand(
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
            Required = true,
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
            Required = true,
        };

        _breakDurationOption = new Option<string>("--break-duration", "-b")
        {
            Description = "The duration of a break time.",
            Arity = ArgumentArity.ExactlyOne,
            Required = true,
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
        string targetCyclesString = parseResult.GetRequiredValue(_targetCyclesOption);
        if (!uint.TryParse(targetCyclesString, out uint targetCycles))
        {
            await _messageWriter.WriteAsync(
                error: true,
                message: "Target cycles must be a valid positive number.",
                cancellationToken);
            return;
        }

        string focusDuration = parseResult.GetRequiredValue(_focusDurationOption);
        string breakDuration = parseResult.GetRequiredValue(_breakDurationOption);
        string? delayBetweenTimes = parseResult.GetValue(_delayOption);
        delayBetweenTimes = !string.IsNullOrWhiteSpace(delayBetweenTimes) ? delayBetweenTimes : "0s";

        var command = IpcCommand.CreateNew(sessionConfigId, targetCycles, focusDuration, breakDuration, delayBetweenTimes);

        Response? response = await _commandSender.SendAsync(command, cancellationToken);

        if (response is not null)
        {
            await _messageWriter.WriteAsync(error: !response.Success, response.Message, cancellationToken);
        }
    }
}
