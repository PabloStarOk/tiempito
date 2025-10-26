using System.CommandLine;

using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Models;

using Command = System.CommandLine.Command;

namespace Tiempito.CLI.Commands.Config;

/// <summary>
/// Represents the command to enable a user's feature configuration.
/// </summary>
public class GenericFeatureConfigCommand : Command
{
    private readonly ICommandSender _commandSender;
    private readonly IMessageWriter _messageWriter;
    private readonly string _commandParent;
    private readonly string[] _allowedFeatureArgs = ["nc", "notification"];
    private readonly Argument<string> _featureArgument;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericFeatureConfigCommand"/> class.
    /// </summary>
    /// <param name="commandSender">The sender used to execute session commands.</param>
    /// <param name="messageWriter">The writer used to output messages to the terminal.</param>
    /// <param name="commandParent">Command parent of this command.</param>
    /// <param name="featureArgument">Argument that will contain the feature to enable.</param>
    /// <param name="name">Name of the command.</param>
    /// <param name="description">Description of the command.</param>
    public GenericFeatureConfigCommand(
        ICommandSender commandSender,
        IMessageWriter messageWriter,
        string commandParent,
        Argument<string> featureArgument,
        string name,
        string description)
        : base(name, description)
    {
        _commandSender = commandSender;
        _messageWriter = messageWriter;
        _commandParent = commandParent;
        _featureArgument = featureArgument;
        _featureArgument.AcceptOnlyFromAmong(_allowedFeatureArgs);
        Add(_featureArgument);
        SetAction(ExecuteAsync);
    }

    private async Task ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var arguments = new Dictionary<string, string>
        {
            { "feature", parseResult.GetRequiredValue(_featureArgument) },
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
