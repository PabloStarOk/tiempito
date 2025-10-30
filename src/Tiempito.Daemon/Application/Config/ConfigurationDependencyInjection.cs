using Tiempito.Daemon.Application.Config.Sessions;
using Tiempito.Daemon.Application.Config.User;
using Tiempito.Daemon.Infrastructure.Config.Sessions;
using Tiempito.Daemon.Infrastructure.Config.User;

namespace Tiempito.Daemon.Application.Config;

/// <summary>
/// Dependency injection for configuration services.
/// </summary>
public static class ConfigurationDependencyInjection
{
    /// <summary>
    /// Add all the services required for the configuration of the
    /// daemon.
    /// </summary>
    /// <param name="serviceCollection">Collection of services.</param>
    /// <returns>Collection with new added services.</returns>
    public static IServiceCollection AddConfigurationServices(
        this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IUserConfigReader, UserConfigReader>();
        serviceCollection.AddSingleton<IUserConfigWriter, UserConfigWriter>();
        serviceCollection.AddSingleton<ISessionConfigReader, SessionConfigReader>();
        serviceCollection.AddSingleton<ISessionConfigWriter, SessionConfigWriter>();
        
        serviceCollection.AddSingleton<UserConfigService>();
        serviceCollection.AddSingleton<SessionConfigService>();
        
        serviceCollection.AddSingleton<IUserConfigService>(sp => sp.GetRequiredService<UserConfigService>());
        serviceCollection.AddSingleton<ISessionConfigService>(sp => sp.GetRequiredService<SessionConfigService>());

        serviceCollection.AddSingleton<IHostedService>(sp => sp.GetRequiredService<UserConfigService>());
        serviceCollection.AddSingleton<IHostedService>(sp => sp.GetRequiredService<SessionConfigService>());
        
        return serviceCollection;
    }
}