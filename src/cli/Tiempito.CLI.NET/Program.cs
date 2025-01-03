using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.IO.Pipes;
using System.Text;
using Tiempito.CLI.NET.Client;
using Tiempito.CLI.NET.Config;
using Tiempito.CLI.NET.Session;
using Tiempito.IPC.NET.Packets;

// Session Commands
var rootCommand = new RootCommand("Tiempito CLI");
var builder = new CommandLineBuilder(rootCommand);

builder.UseDefaults();

// TODO: Improve DI
var pipeClient = new NamedPipeClientStream(".", "tiempito-pipe", PipeDirection.InOut); // TODO: Read config of the host.
IAsyncPacketHandler packetHandler = new PipePacketHandler(Encoding.UTF8); // TODO: Read config of the host for the encoding.
IPacketSerializer packetSerializer = new PacketSerializer();
IPacketDeserializer packetDeserializer = new PacketDeserializer();
IClient client = new Client(pipeClient, packetHandler, packetSerializer, packetDeserializer);
IAsyncCommandExecutor asyncCommandExecutor = new CommandExecutor(client);

Command sessionCommand = new SessionCommand(asyncCommandExecutor).GetCommand();
Command configCommand = new ConfigCommand(asyncCommandExecutor).GetCommand();

rootCommand.AddCommand(sessionCommand);
rootCommand.AddCommand(configCommand);

Parser parser = builder.Build();
await parser.InvokeAsync(args);
