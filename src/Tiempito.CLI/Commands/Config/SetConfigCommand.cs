using System.CommandLine;

using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Models;

using Command = System.CommandLine.Command;

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
    private readonly string _commandParent;
    private readonly Option<string> _defaultSessionIdOption;

    /// <summary>
    /// Instantiates a <see cref="SetConfigCommand"/>.
    /// </summary>
    /// <param name="commandSender">The sender used to execute session commands.</param>
    /// <param name="messageWriter">The writer used to output messages to the terminal.</param>
    /// <param name="commandParent">Command parent of this command.</param>
    /// <param name="defaultSessionIdOption">An option to change the default session of the user.</param>
    public SetConfigCommand(
        ICommandSender commandSender,
        IMessageWriter messageWriter,
        string commandParent, Option<string> defaultSessionIdOption) 
        : base(CommandName, CommandDescription)
    {
        _commandSender = commandSender;
        _messageWriter = messageWriter;
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
