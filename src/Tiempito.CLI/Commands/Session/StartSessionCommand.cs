using System.CommandLine;

using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Models;

using Command = System.CommandLine.Command;
using IpcStartSessionCommand = Tiempito.IPC.Models.Commands.Session.StartSessionCommand;

namespace Tiempito.CLI.Commands.Session;

/// <summary>
/// Starts a new session.
/// </summary>
public class StartSessionCommand : Command
{
    private const string CommandName = "start";
    private const string CommandDescription = "Starts a new session.";

    private readonly ICommandSender _commandSender;
    private readonly IMessageWriter _messageWriter;
    private readonly ISessionFollower _sessionFollower;
    private readonly Option<string> _sessionIdOption;
    private readonly Option<string> _sessionConfigIdOption;
    private readonly Option<bool>? _followOption;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartSessionCommand"/> class.
    /// </summary>
    /// <param name="commandSender">The sender used to execute session commands.</param>
    /// <param name="messageWriter">The writer used to output messages to the terminal.</param>
    /// <param name="sessionFollower">The follower used to track session progress.</param>
    /// <param name="sessionIdOption">Session id option.</param>
    /// <param name="followOption">Indicates whether to follow session progress.</param>
    public StartSessionCommand(
        ICommandSender commandSender,
        IMessageWriter messageWriter,
        ISessionFollower sessionFollower,
        Option<string> sessionIdOption,
        Option<bool> followOption)
        : base(CommandName, CommandDescription)
    {
        _commandSender = commandSender;
        _messageWriter = messageWriter;
        _sessionFollower = sessionFollower;
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
        var sessionId = parseResult.GetValue(_sessionIdOption) ?? string.Empty;
        var sessionConfigId = parseResult.GetValue(_sessionConfigIdOption) ?? string.Empty;
        var command = IpcStartSessionCommand.CreateNew(sessionId, sessionConfigId);

        Response? response = await _commandSender.SendAsync(command, cancellationToken);

        if (response is not null)
        {
            await _messageWriter.WriteAsync(error: !response.Success, response.Message, cancellationToken);
        }

        var follow = _followOption is not null && parseResult.GetValue(_followOption);
        if (follow && response?.Success == true)
        {
            _sessionFollower.MustFollow = true;
        }
    }
}
