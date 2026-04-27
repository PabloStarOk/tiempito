using System.CommandLine;

using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Models;
using Tiempito.IPC.Models.Commands.Session;

using Command = System.CommandLine.Command;

namespace Tiempito.CLI.Commands.Session;

/// <summary>
/// Represents a generic session command that uses only the session id as argument.
/// </summary>
/// <typeparam name="TCommand">The type of session command to execute.</typeparam>
public class GenericSessionCommand<TCommand> : Command
    where TCommand : IPC.Models.Commands.Command
{
    private readonly ICommandSender _commandSender;
    private readonly IMessageWriter _messageWriter;
    private readonly ISessionFollower _sessionFollower;
    private readonly Option<string> _sessionIdOption;
    private readonly Option<bool>? _followOption;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericSessionCommand{TCommand}"/> class.
    /// </summary>
    /// <param name="commandSender">The sender used to execute session commands.</param>
    /// <param name="messageWriter">The writer used to output messages to the terminal.</param>
    /// <param name="sessionFollower">The follower used to track session progress.</param>
    /// <param name="sessionIdOption">Session id option.</param>
    /// <param name="name">Name of the command.</param>
    /// <param name="description">Description of the command.</param>
    /// <param name="followOption">Indicates whether to follow session progress.</param>
    public GenericSessionCommand(
        ICommandSender commandSender,
        IMessageWriter messageWriter,
        ISessionFollower sessionFollower,
        Option<string> sessionIdOption,
        string name,
        string description,
        Option<bool>? followOption = null)
        : base(name, description)
    {
        _commandSender = commandSender;
        _messageWriter = messageWriter;
        _sessionFollower = sessionFollower;
        _sessionIdOption = sessionIdOption;
        _followOption = followOption;

        _sessionIdOption.Required = false;
        Add(_sessionIdOption);

        if (_followOption != null)
        {
            Add(_followOption);
        }

        SetAction(ExecuteAsync);
    }

    private async Task ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var sessionId = parseResult.GetValue(_sessionIdOption) ?? string.Empty;

        IPC.Models.Commands.Command command = typeof(TCommand) switch
        {
            { } t when t == typeof(CancelSessionCommand) => CancelSessionCommand.CreateNew(sessionId),
            { } t when t == typeof(PauseSessionCommand) => PauseSessionCommand.CreateNew(sessionId),
            { } t when t == typeof(ResumeSessionCommand) => ResumeSessionCommand.CreateNew(sessionId),
            _ => throw new InvalidOperationException($"Unsupported command type: {typeof(TCommand).FullName}"),
        };

        Response response = await _commandSender.SendAsync(command, cancellationToken);
        await _messageWriter.WriteLineAsync(error: !response.Success, response.Message, cancellationToken);

        var follow = _followOption is not null && parseResult.GetValue(_followOption);
        if (follow && response.Success)
        {
            _sessionFollower.MustFollow = true;
        }
    }
}
