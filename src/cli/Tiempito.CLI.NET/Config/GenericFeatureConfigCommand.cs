using System.CommandLine;
using Tiempito.CLI.NET.Client;

namespace Tiempito.CLI.NET.Config;

/// <summary>
/// Represents the command to enable a user's feature configuration.
/// </summary>
public class GenericFeatureConfigCommand : Command
{
    private readonly IAsyncCommandExecutor _asyncCommandExecutor;
    private readonly string _commandParent;
    private readonly string[] _allowedFeatureArgs = ["nc", "notification"];
    
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
        
        featureArgument.FromAmong(_allowedFeatureArgs);
        AddArgument(featureArgument);
        this.SetHandler(CommandHandler, featureArgument);
    }

    /// <summary>
    /// Sends the request to execute a command to enable/disable a feature.
    /// </summary>
    /// <param name="feature">Feature to enable/disable.</param>
    private async Task CommandHandler(string feature)
    {
        var arguments = new Dictionary<string, string>
        {
            { "feature", feature }
        };
        await _asyncCommandExecutor.ExecuteAsync(_commandParent, subcommand: Name, arguments);
    }
}
