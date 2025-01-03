using System.CommandLine;
using Tiempito.CLI.NET.Client;
using Tiempito.IPC.NET.Messages;

namespace Tiempito.CLI.NET.Config;

/// <summary>
/// Represents the command to enable a user's feature configuration.
/// </summary>
public class EnableConfigCommand : Command
{
    private readonly string _commandParent;
    private readonly IClient _client;
    private readonly string[] _allowedFeatureArgs = ["nc", "notification"];
    
    /// <summary>
    /// Instantiates a <see cref="EnableConfigCommand"/>.
    /// </summary>
    /// <param name="client">Client to send the request to the daemon.</param>
    /// <param name="commandParent">Parent of the command.</param>
    /// <param name="featureArgument">Argument that will contain the feature to enable.</param>
    public EnableConfigCommand(IClient client, string commandParent, Argument<string> featureArgument) 
        : base ("enable", "Enables a specified feature in the user's configuration.")
    {
        _client = client;
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
        
        await _client.SendRequestAsync(new Request(_commandParent, Name, arguments));
        Response response = await _client.ReceiveResponseAsync();
        Console.WriteLine(response.Message);
    }
}
