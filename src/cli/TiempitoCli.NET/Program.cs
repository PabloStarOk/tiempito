using System.CommandLine;
using System.IO.Pipes;
using System.Text;
using TiempitoCli.NET.Client;
using Tiempito.IPC.NET.Messages;
using Tiempito.IPC.NET.Packets;

// Session Commands
const string host = ".";
const string pipeName = "tiempito-pipe"; // TODO: Read config of the host.
const PipeDirection pipeDirection = PipeDirection.InOut; // TODO: Read config of the host.
var pipeClient = new NamedPipeClientStream(host, pipeName, pipeDirection);
var encoding = new UTF8Encoding(); // TODO: Read config of the host.
var pipeMessageHandler = new PipePacketHandler(encoding);
var packetSerializer = new PacketSerializer();
var packetDeserializer = new PacketDeserializer();
var client = new Client(pipeClient, pipeMessageHandler, packetSerializer, packetDeserializer, 3000);

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