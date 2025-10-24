using System.CommandLine;

using Tiempito.CLI.Client.Interfaces;

namespace Tiempito.CLI.Config;

/// <summary>
/// Represents the command to enable a user's feature configuration.
/// </summary>
public class GenericFeatureConfigCommand : Command
{
    private readonly IAsyncCommandExecutor _asyncCommandExecutor;
    private readonly string _commandParent;
    private readonly string[] _allowedFeatureArgs = ["nc", "notification"];
    private readonly Argument<string> _featureArgument;

    /// <summary>
    /// Instantiates a <see cref="GenericFeatureConfigCommand"/>.
    /// </summary>
    /// <param name="asyncCommandExecutor">An asynchronous executor of commands.</param>
    /// <param name="commandParent">Command parent of this command.</param>
    /// <param name="featureArgument">Argument that will contain the feature to enable.</param>
    /// <param name="name">Name of the command.</param>
    /// <param name="description">Description of the command.</param>
    public GenericFeatureConfigCommand(
        IAsyncCommandExecutor asyncCommandExecutor,
        string commandParent, Argument<string> featureArgument,
        string name,string description) 
        : base (name, description)
    {
        _asyncCommandExecutor = asyncCommandExecutor;
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
        await _asyncCommandExecutor.ExecuteAsync(_commandParent, subcommand: Name, arguments);
    }
}
