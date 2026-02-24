using System.CommandLine;

using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Models;

using Command = System.CommandLine.Command;
using IpcSetConfigCommand = Tiempito.IPC.Models.Commands.Config.SetConfigCommand;

namespace Tiempito.CLI.Commands.Config;

/// <summary>
/// Represents the subcommand to modify parameters of the user's configuration.
/// </summary>
public class SetConfigCommand : Command
{
    private const string CommandName = "set";
    private const string CommandDescription = "Sets the specified user configuration.";

    private readonly ICommandSender _commandSender;
    private readonly IMessageWriter _messageWriter;
    private readonly Option<string> _defaultSessionIdOption;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetConfigCommand"/> class.
    /// </summary>
    /// <param name="commandSender">The sender used to execute session commands.</param>
    /// <param name="messageWriter">The writer used to output messages to the terminal.</param>
    /// <param name="defaultSessionIdOption">An option to change the default session of the user.</param>
    public SetConfigCommand(
        ICommandSender commandSender,
        IMessageWriter messageWriter,
        Option<string> defaultSessionIdOption)
        : base(CommandName, CommandDescription)
    {
        _commandSender = commandSender;
        _messageWriter = messageWriter;
        _defaultSessionIdOption = defaultSessionIdOption;
        _defaultSessionIdOption.Required = false;
        Add(_defaultSessionIdOption);
        SetAction(ExecuteAsync);
    }

    private async Task ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var defaultSessionConfigId = parseResult.GetRequiredValue(_defaultSessionIdOption);
        var command = IpcSetConfigCommand.CreateNew(defaultSessionConfigId);
        Response? response = await _commandSender.SendAsync(command, cancellationToken);

        if (response is not null)
        {
            await _messageWriter.WriteLineAsync(error: !response.Success, response.Message, cancellationToken);
        }
    }
}
