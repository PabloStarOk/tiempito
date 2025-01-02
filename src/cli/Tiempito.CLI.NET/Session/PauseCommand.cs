using System.CommandLine;
using Tiempito.CLI.NET.Client;
using Tiempito.IPC.NET.Messages;

namespace Tiempito.CLI.NET.Session;

public class PauseCommand : Command
{
    private readonly IClient _client;
    
    public PauseCommand(
        IClient client, Option<string> sessionIdOption,
        string name, string description) : base(name, description)
    {
        _client = client;
        this.SetHandler(CommandHandler, sessionIdOption);
    }
    
    private async Task CommandHandler(string sessionId)
    {
        var arguments = new Dictionary<string, string>
        {
            { "sessionId", sessionId }
        };
        await _client.SendRequestAsync(new Request(CommandType: "session", SubcommandType: "pause", arguments));
        Response response = await _client.ReceiveResponseAsync();
        Console.WriteLine(response.Message);
    }
}
