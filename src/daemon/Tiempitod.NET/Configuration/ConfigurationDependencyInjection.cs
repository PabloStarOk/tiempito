using Tiempitod.NET.Common;
using Tiempitod.NET.Configuration.Session;
using Tiempitod.NET.Configuration.User;

namespace Tiempitod.NET.Configuration;

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
        serviceCollection.AddSingleton<SessionConfigProvider>();

        using var serviceProvider = serviceCollection.BuildServiceProvider();
        
        serviceCollection.AddSingleton<IUserConfigService>(sp => sp.GetRequiredService<UserConfigService>());
        serviceCollection.AddSingleton<ISessionConfigProvider>(sp => sp.GetRequiredService<SessionConfigProvider>());
        
        serviceCollection.AddSingleton<Service>(sp => sp.GetRequiredService<UserConfigService>());
        serviceCollection.AddSingleton<Service>(sp => sp.GetRequiredService<SessionConfigProvider>());
        
        return serviceCollection;
    }
}