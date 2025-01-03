using System.CommandLine;
using Tiempito.CLI.NET.Client;
using Tiempito.IPC.NET.Messages;

namespace Tiempito.CLI.NET.Config;

/// <summary>
/// Represents the subcommand to modify parameters of the user's configuration.
/// </summary>
public class SetConfigCommand : Command
{
    private readonly IClient _client;
    
    /// <summary>
    /// Instantiates a <see cref="SetConfigCommand"/>.
    /// </summary>
    /// <param name="client">Client connection to send the request to the daemon.</param>
    /// <param name="defaultSessionIdOption">An option to change the default session of the user.</param>
    public SetConfigCommand(
        IClient client, Option<string> defaultSessionIdOption) 
        : base("set", "Sets the specified user configuration.")
    {
        _client = client;
        defaultSessionIdOption.IsRequired = false;
        AddOption(defaultSessionIdOption);
        this.SetHandler(CommandHandler, defaultSessionIdOption);
    }
    
    /// <summary>
    /// Sends a request to the daemon to modify the provided arguments.
    /// </summary>
    /// <param name="defaultSessionId">New ID of the default session of the user.</param>
    private async Task CommandHandler(string defaultSessionId)
    {
        var arguments = new Dictionary<string, string>
        {
            { "default-session-id", defaultSessionId }
        };
        await _client.SendRequestAsync(new Request(CommandType: "config", SubcommandType: Name, arguments));
        Response response = await _client.ReceiveResponseAsync();
        Console.WriteLine(response.Message);
    }
}
