using System.CommandLine;
using Tiempito.CLI.NET.Client;
using Tiempito.IPC.NET.Messages;

namespace Tiempito.CLI.NET.Session;

public class CancelCommand : Command
{
    private readonly IClient _client;
    
    public CancelCommand(
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
            { "session-id", sessionId }
        };
        await _client.SendRequestAsync(new Request(CommandType: "session", SubcommandType: "cancel", arguments));
        Response response = await _client.ReceiveResponseAsync();
        Console.WriteLine(response.Message);
    }
}
