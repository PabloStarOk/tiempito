using System.CommandLine;
using System.IO.Pipes;

using Microsoft.Extensions.DependencyInjection;

using Tiempito.CLI.Client;
using Tiempito.CLI.Client.Interfaces;
using Tiempito.CLI.Commands.Config;
using Tiempito.CLI.Commands.Session;

namespace Tiempito.CLI.Commands;

/// <summary>
/// Provides registration helpers to add command-related services to the dependency injection container.
/// </summary>
internal static class DependencyInjection
{
    /// <summary>
    /// Registers the root <see cref="System.CommandLine.RootCommand"/> factory and related commands.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    public static void AddRootCommand(this IServiceCollection services)
    {
        AddClient(services);
        AddCommands(services);
        services.AddTransient(sp =>
        {
            var rootCommand = new RootCommand("Tiempito CLI");
            var commands = sp.GetRequiredService<IEnumerable<Command>>();
            foreach (Command command in commands)
            {
                rootCommand.Add(command);
            }

            return rootCommand;
        });
    }

    private static void AddCommands(IServiceCollection services)
    {
        services.AddTransient<IAsyncCommandExecutor>(sp =>
        {
            var client = sp.GetRequiredService<IClient>();
            return new CommandExecutor(client, Console.Out, Console.Error);
        });
        services.AddTransient<Command, SessionCommand>();
        services.AddTransient<Command, ConfigCommand>();
    }

    private static void AddClient(IServiceCollection services)
    {
        services.AddSingleton(_ => new NamedPipeClientStream(
                ".",
                "tiempito-pipe",
                PipeDirection.InOut,
                PipeOptions.Asynchronous)); // TODO: Read config of the host.
        services.AddSingleton<TextReader>(sp =>
        {
            var client = sp.GetRequiredService<NamedPipeClientStream>();
            return new StreamReader(client);
        });
        services.AddSingleton<IClient, Client.Client>();
    }
}