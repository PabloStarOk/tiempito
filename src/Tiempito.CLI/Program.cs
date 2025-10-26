using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

using Tiempito.CLI;
using Tiempito.CLI.Commands;
using Tiempito.CLI.Services;
using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC;

var services = new ServiceCollection();
services.AddSingleton<CancellationTokenSource>();
services.AddSingleton<ApplicationLifetime>();
services.AddIpc();
services.AddServices();
services.AddRootCommand();
await using var sp = services.BuildServiceProvider();
await using var serviceScope = sp.CreateAsyncScope();
var rootCommand = serviceScope.ServiceProvider.GetRequiredService<RootCommand>();
var sessionFollower = serviceScope.ServiceProvider.GetRequiredService<ISessionFollower>();
var appLifetime = serviceScope.ServiceProvider.GetRequiredService<ApplicationLifetime>();

var parseResult = rootCommand.Parse(args);

int exitCode = await parseResult.InvokeAsync(cancellationToken: appLifetime.Token);
if (exitCode != 0)
{
    return exitCode;
}

await sessionFollower.FollowAsync(appLifetime.Token);
return 0;
