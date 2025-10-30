using System.IO.Pipes;

using Microsoft.Extensions.Options;

using Tiempito.Daemon.Server.Configuration;
using Tiempito.IPC;

namespace Tiempito.Daemon.Server;

/// <summary>
/// Provides extension methods for registering server-related services in the dependency injection container.
/// </summary>
internal static class DependencyInjection
{
    /// <summary>
    /// Registers the server services and configuration into the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the server services to.</param>
    /// <param name="configuration">The application configuration used for server setup.</param>
    public static void AddServer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PipeConfig>(configuration.GetRequiredSection(PipeConfig.Pipe));
        services.AddIpc();
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PipeConfig>>();
            return new NamedPipeServerStream(
                options.Value.PipeName,
                options.Value.PipeDirection,
                options.Value.PipeMaxInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);
        });

        services.AddHostedService<Server>();
    }
}