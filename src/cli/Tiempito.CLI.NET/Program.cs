using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.IO.Pipes;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using Tiempito.CLI.NET.Client;
using Tiempito.CLI.NET.Config;
using Tiempito.CLI.NET.Session;
using Tiempito.IPC.NET.Packets;

// Session Commands
var rootCommand = new RootCommand("Tiempito CLI");
var builder = new CommandLineBuilder(rootCommand);

builder.UseDefaults();

// TODO: Improve DI

var jsonSerializerOptions = new JsonSerializerOptions()
{
    TypeInfoResolver = IpcSerializerContext.Default
};
IAsyncPacketHandler packetHandler = new PipePacketHandler(Encoding.UTF8, jsonSerializerOptions); // TODO: Read config of the host for the encoding.
IPacketSerializer packetSerializer = new PacketSerializer(jsonSerializerOptions);
IPacketDeserializer packetDeserializer = new PacketDeserializer(jsonSerializerOptions);

var pipeClient = new NamedPipeClientStream(
    ".",
    "tiempito-pipe",
    PipeDirection.InOut,
    PipeOptions.Asynchronous); // TODO: Read config of the host.
var pipeStdIn = new StreamReader(pipeClient);
IClient client = new Client(pipeClient, packetHandler, packetSerializer, packetDeserializer, pipeStdIn);
IAsyncCommandExecutor asyncCommandExecutor = new CommandExecutor(client, Console.Out, Console.Error);

Command sessionCommand = new SessionCommand(asyncCommandExecutor).GetCommand();
Command configCommand = new ConfigCommand(asyncCommandExecutor).GetCommand();

rootCommand.AddCommand(sessionCommand);
rootCommand.AddCommand(configCommand);

Parser parser = builder.Build();
await parser.InvokeAsync(args);
