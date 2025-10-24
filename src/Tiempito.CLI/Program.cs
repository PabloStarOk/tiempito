using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

using Tiempito.CLI.Commands;
using Tiempito.IPC;

var services = new ServiceCollection();
services.AddIpc();
services.AddRootCommand();
await using var sp = services.BuildServiceProvider();
await using var serviceScope = sp.CreateAsyncScope();
var rootCommand = serviceScope.ServiceProvider.GetRequiredService<RootCommand>();
var parseResult = rootCommand.Parse(args);
await parseResult.InvokeAsync();
