using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.IO.Pipes;

using Microsoft.Extensions.DependencyInjection;

using Tiempito.CLI.Client;
using Tiempito.CLI.Client.Interfaces;
using Tiempito.CLI.Config;
using Tiempito.CLI.Session;
using Tiempito.IPC;
using Tiempito.IPC.Abstractions;

// Session Commands
var rootCommand = new RootCommand("Tiempito CLI");
var builder = new CommandLineBuilder(rootCommand);

builder.UseDefaults();

// TODO: Improve DI

var pipeClient = new NamedPipeClientStream(
    ".",
    "tiempito-pipe",
    PipeDirection.InOut,
    PipeOptions.Asynchronous); // TODO: Read config of the host.
var pipeStdIn = new StreamReader(pipeClient);

var services = new ServiceCollection();
services.AddIpc();
await using var sp = services.BuildServiceProvider();
var transportWriter = sp.GetRequiredService<IMessageWriter>();
var transportReader = sp.GetRequiredService<IMessageReader>();

IClient client = new Client(pipeClient, pipeStdIn, transportWriter, transportReader);
IAsyncCommandExecutor asyncCommandExecutor = new CommandExecutor(client, Console.Out, Console.Error);

Command sessionCommand = new SessionCommand(asyncCommandExecutor).GetCommand();
Command configCommand = new ConfigCommand(asyncCommandExecutor).GetCommand();

rootCommand.AddCommand(sessionCommand);
rootCommand.AddCommand(configCommand);

Parser parser = builder.Build();
await parser.InvokeAsync(args);
