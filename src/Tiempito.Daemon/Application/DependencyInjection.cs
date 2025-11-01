using Tiempito.Daemon.Application.Commands;
using Tiempito.Daemon.Application.Commands.Config;
using Tiempito.Daemon.Application.Commands.Sessions;
using Tiempito.Daemon.Application.Config.Sessions;
using Tiempito.Daemon.Application.Config.User;
using Tiempito.Daemon.Application.Notifications;
using Tiempito.Daemon.Application.Sessions;

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
    /// <param name="configuration">The application configuration to use for service setup.</param>
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        AddConfigServices(services);
        AddCommandDispatcher(services);
        AddNotificationServices(services, configuration);
        AddSessionServices(services);
    }

    private static void AddCommandDispatcher(IServiceCollection services)
    {
        services.AddTransient<ICommandHandler, StartSessionCommandHandler>();
        services.AddTransient<ICommandHandler, CancelSessionCommandHandler>();
        services.AddTransient<ICommandHandler, PauseSessionCommandHandler>();
        services.AddTransient<ICommandHandler, ResumeSessionCommandHandler>();
        services.AddTransient<ICommandHandler, CreateSessionConfigCommandHandler>();
        services.AddTransient<ICommandHandler, ModifySessionConfigCommandHandler>();
        services.AddTransient<ICommandHandler, UserFeatureConfigCommandHandler>();
        services.AddTransient<ICommandHandler, SetConfigCommandHandler>();
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
    }

    private static void AddConfigServices(IServiceCollection services)
    {
        services.AddSingleton<UserConfigService>();
        services.AddSingleton<IUserConfigService>(sp => sp.GetRequiredService<UserConfigService>());
        services.AddHostedService(sp => sp.GetRequiredService<UserConfigService>());

        services.AddSingleton<SessionConfigService>();
        services.AddSingleton<ISessionConfigService>(sp => sp.GetRequiredService<SessionConfigService>());
        services.AddHostedService(sp => sp.GetRequiredService<SessionConfigService>());
    }

    private static void AddNotificationServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<NotificationConfig>(configuration.GetSection(NotificationConfig.Notification));
    }

    private static void AddSessionServices(IServiceCollection services)
    {
        services.AddSingleton<ISessionService, SessionService>();
    }
}