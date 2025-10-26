using System.IO.Pipes;

using Microsoft.Extensions.DependencyInjection;

using Tiempito.CLI.Services.Abstractions;
using Tiempito.CLI.Services.Implementations;

namespace Tiempito.CLI.Services;

/// <summary>
/// Provides extension methods for registering application services in the dependency injection container.
/// </summary>
internal static class DependencyInjection
{
    /// <summary>
    /// Registers application services with the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    public static void AddServices(this IServiceCollection services)
    {
        AddClient(services);
        services.AddScoped<ICommandSender, CommandSender>();
        services.AddSingleton<ISessionFollower, SessionFollower>();
        services.AddSingleton<IMessageWriter>(_ => new MessageWriter(Console.Out, Console.Error));
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
        services.AddSingleton<IClient, Client>();
    }
}