using Tiempito.Daemon.Application.Commands;
using Tiempito.Daemon.Application.Commands.Config;
using Tiempito.Daemon.Application.Commands.Sessions;
using Tiempito.Daemon.Application.Config.User;
using Tiempito.Daemon.Application.Sessions;
using Tiempito.Daemon.Infrastructure.Sessions;

namespace Tiempito.Daemon.Application;

/// <summary>
/// Provides extension methods for registering application dependencies in the service collection.
/// </summary>
internal static class DependencyInjection
{
    /// <summary>
    /// Registers application-level services and command handlers in the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add dependencies to.</param>
    public static void AddApplication(this IServiceCollection services)
    {
        AddSessionServices(services);
        AddCommandDispatcher(services);
    }

    private static void AddCommandDispatcher(IServiceCollection services)
    {
        services.AddTransient<ICommandHandler, StartSessionCommandHandler>();
        services.AddTransient<ICommandHandler, CancelSessionCommandHandler>();
        services.AddTransient<ICommandHandler, PauseSessionCommandHandler>();
        services.AddTransient<ICommandHandler, ResumeSessionCommandHandler>();
        services.AddTransient<ICommandHandler, CreateSessionConfigCommandHandler>();
        services.AddTransient<ICommandHandler, ModifySessionConfigCommandHandler>();
        AddUserFeatureConfigCommandHandler(services, enable: true);
        AddUserFeatureConfigCommandHandler(services, enable: false);
        services.AddTransient<ICommandHandler, SetConfigCommandHandler>();
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
    }

    private static void AddUserFeatureConfigCommandHandler(IServiceCollection services, bool enable)
    {
        services.AddTransient<ICommandHandler>(sp =>
        {
            var userConfigService = sp.GetRequiredService<IUserConfigService>();
            return new UserFeatureConfigCommandHandler(userConfigService, enable);
        });
    }

    private static void AddSessionServices(IServiceCollection services)
    {
        services.AddSingleton<ISessionFactory, SessionFactory>();
    }
}