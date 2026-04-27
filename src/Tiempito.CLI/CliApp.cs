using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

using Tiempito.CLI.Commands;
using Tiempito.CLI.Services;
using Tiempito.CLI.Services.Abstractions;
using Tiempito.CLI.Services.Implementations;
using Tiempito.IPC;

namespace Tiempito.CLI;

/// <summary>
/// Represents the entry point for the Tiempito CLI application.
/// </summary>
internal sealed class CliApp
{
    private readonly IServiceCollection _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="CliApp"/> class.
    /// </summary>
    /// <param name="services">The DI service collection to use for application services.</param>
    public CliApp(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Runs the CLI application asynchronously with the provided command-line arguments.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The exit code of the application.</returns>
    public async Task<int> RunAsync(string[] args)
    {
        AddDiServices();
        await using var sp = _services.BuildServiceProvider();
        await using var serviceScope = sp.CreateAsyncScope();
        var rootCommand = serviceScope.ServiceProvider.GetRequiredService<RootCommand>();
        var sessionFollower = serviceScope.ServiceProvider.GetRequiredService<ISessionFollower>();
        var appLifetime = serviceScope.ServiceProvider.GetRequiredService<IApplicationLifetime>();

        var parseResult = rootCommand.Parse(args);

        int exitCode = await parseResult.InvokeAsync(cancellationToken: appLifetime.StoppingToken);
        if (exitCode != 0)
        {
            return exitCode;
        }

        await sessionFollower.FollowAsync(appLifetime.StoppingToken);
        appLifetime.StopApplication();
        return 0;
    }

    private void AddDiServices()
    {
        _services.AddSingleton<CancellationTokenSource>();
        _services.AddSingleton<IApplicationLifetime, ConsoleApplicationLifetime>();
        _services.AddIpc();
        _services.AddServices();
        _services.AddRootCommand();
    }
}