using System.CommandLine;

using Microsoft.Extensions.DependencyInjection;

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
        services.AddTransient<Command, SessionCommand>();
        services.AddTransient<Command, ConfigCommand>();
    }
}