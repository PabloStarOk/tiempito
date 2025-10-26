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
        services.AddScoped<ICommandSender, CommandSender>();
        services.AddSingleton<ISessionFollower, SessionFollower>();
        services.AddSingleton<IMessageWriter>(_ => new MessageWriter(Console.Out, Console.Error));
    }
}