using System.CommandLine;
using Tiempito.CLI.NET.Client;

namespace Tiempito.CLI.NET.Config;

/// <summary>
/// Represents the command to disable a user's feature configuration.
/// </summary>
public class DisableConfigCommand : Command
{
    private readonly IAsyncCommandExecutor _asyncCommandExecutor;
    private readonly string _commandParent;
    private readonly string[] _allowedFeatureArgs = ["nc", "notification"]; // TODO: Duplicated variable.
    
    /// <summary>
    /// Instantiates a <see cref="DisableConfigCommand"/>.
    /// </summary>
    /// <param name="asyncCommandExecutor">An asynchronous executor of commands.</param>
    /// <param name="commandParent">Command parent of this command.</param>
    /// <param name="featureArgument">Argument that will contain the feature to enable.</param>
    public DisableConfigCommand(
        IAsyncCommandExecutor asyncCommandExecutor,
        string commandParent, Argument<string> featureArgument) 
        : base ("disable", "Disables a specified feature in the user's configuration.")
    {
        _asyncCommandExecutor = asyncCommandExecutor;
        _commandParent = commandParent;
        featureArgument.FromAmong(_allowedFeatureArgs);
        AddArgument(featureArgument);
        this.SetHandler(CommandHandler, featureArgument);
    }
    
    /// <summary>
    /// Sends the request to enable the given feature.
    /// </summary>
    /// <param name="feature">Feature to enable.</param>
    private async Task CommandHandler(string feature)
    {
        var arguments = new Dictionary<string, string>
        {
            { "feature", feature }
        };
        await _asyncCommandExecutor.ExecuteAsync(_commandParent, subcommand: Name, arguments);
    }
}
