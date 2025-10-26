using System.CommandLine;

using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Models;

using Command = System.CommandLine.Command;

namespace Tiempito.CLI.Commands.Session;

/// <summary>
/// Represents a generic session command that uses only the session id as argument.
/// </summary>
public class GenericSessionCommand : Command
{
    private readonly ICommandSender _commandSender;
    private readonly IMessageWriter _messageWriter;
    private readonly ISessionFollower _sessionFollower;
    private readonly string _commandParent;
    private readonly Option<string> _sessionIdOption;
    private readonly Option<bool>? _followOption;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericSessionCommand"/> class.
    /// </summary>
    /// <param name="commandSender">The sender used to execute session commands.</param>
    /// <param name="messageWriter">The writer used to output messages to the terminal.</param>
    /// <param name="sessionFollower">The follower used to track session progress.</param>
    /// <param name="commandParent">Command parent of this command.</param>
    /// <param name="sessionIdOption">Session id option.</param>
    /// <param name="name">Name of the command.</param>
    /// <param name="description">Description of the command.</param>
    /// <param name="followOption">Indicates whether to follow session progress.</param>
    public GenericSessionCommand(
        ICommandSender commandSender,
        IMessageWriter messageWriter,
        ISessionFollower sessionFollower,
        string commandParent,
        Option<string> sessionIdOption,
        string name,
        string description,
        Option<bool>? followOption = null)
        : base(name, description)
    {
        _commandSender = commandSender;
        _messageWriter = messageWriter;
        _sessionFollower = sessionFollower;
        _commandParent = commandParent;
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
        var follow = _followOption is not null && parseResult.GetValue(_followOption);
        var arguments = new Dictionary<string, string>
        {
            { "session-id", parseResult.GetValue(_sessionIdOption) ?? string.Empty },
        };

        Response? response = await _commandSender.SendAsync(_commandParent, Name, arguments, follow, cancellationToken);

        if (response is not null)
        {
            await _messageWriter.WriteAsync(error: !response.Success, response.Message, cancellationToken);
        }

        if (follow && response?.Success == true)
        {
            _sessionFollower.MustFollow = true;
        }
    }
}
