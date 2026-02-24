using System.CommandLine;

using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Models;
using Tiempito.IPC.Models.Enums;

using Command = System.CommandLine.Command;
using IpcUserFeatureConfigCommand = Tiempito.IPC.Models.Commands.Config.UserFeatureConfigCommand;

namespace Tiempito.CLI.Commands.Config;

/// <summary>
/// Represents the command to enable a user's feature configuration.
/// </summary>
public class UserFeatureConfigCommand : Command
{
    private readonly ICommandSender _commandSender;
    private readonly IMessageWriter _messageWriter;
    private readonly Argument<string> _featureArgument;
    private readonly bool _enable;
    private readonly IReadOnlyDictionary<string, UserFeature> _featureAliases = new Dictionary<string, UserFeature>
    {
        { "nc", UserFeature.Notification },
        { "notification", UserFeature.Notification },
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="UserFeatureConfigCommand"/> class.
    /// </summary>
    /// <param name="commandSender">The sender used to execute session commands.</param>
    /// <param name="messageWriter">The writer used to output messages to the terminal.</param>
    /// <param name="featureArgument">Argument that will contain the feature to enable.</param>
    /// <param name="name">Name of the command.</param>
    /// <param name="description">Description of the command.</param>
    /// <param name="enable">If true, the feature will be enabled; if false, it will be disabled.</param>
    public UserFeatureConfigCommand(
        ICommandSender commandSender,
        IMessageWriter messageWriter,
        Argument<string> featureArgument,
        string name,
        string description,
        bool enable)
        : base(name, description)
    {
        _commandSender = commandSender;
        _messageWriter = messageWriter;
        _featureArgument = featureArgument;
        _enable = enable;
        _featureArgument.AcceptOnlyFromAmong(_featureAliases.Keys.ToArray());
        Add(_featureArgument);
        SetAction(ExecuteAsync);
    }

    private async Task ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var userFeatureString = parseResult.GetRequiredValue(_featureArgument);
        var userFeature = _featureAliases[userFeatureString];
        var command = IpcUserFeatureConfigCommand.CreateNew(_enable, userFeature);
        Response? response = await _commandSender.SendAsync(command, cancellationToken);

        if (response is not null)
        {
            await _messageWriter.WriteLineAsync(error: !response.Success, response.Message, cancellationToken);
        }
    }
}
