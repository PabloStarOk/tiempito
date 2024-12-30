using System.CommandLine;
using Tiempito.CLI.NET.Client;
using Tiempito.IPC.NET.Messages;

// Session Commands
var client = Client.Create();

var rootCommand = new RootCommand("Tiempito CLI");
var sessionCommand = new Command("session", "Manage a session.");
var sessionStartSubcommand = new Command("start", "Starts a new session");
sessionStartSubcommand.SetHandler(async () =>
    {
        await client.SendRequestAsync(new Request("start"));
        Response response = await client.ReceiveResponseAsync();
        Console.WriteLine(response.Message);
    }
);

sessionCommand.AddCommand(sessionStartSubcommand);
rootCommand.AddCommand(sessionCommand);
await rootCommand.InvokeAsync(args);