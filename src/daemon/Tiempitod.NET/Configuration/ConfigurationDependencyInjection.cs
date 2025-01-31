using Tiempitod.NET.Common;
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
        serviceCollection.AddSingleton<UserConfigService>();

        using var serviceProvider = serviceCollection.BuildServiceProvider();
        var userConfigService = serviceProvider.GetRequiredService<UserConfigService>();
        
        serviceCollection.AddSingleton<IUserConfigService>(userConfigService);
        serviceCollection.AddSingleton<Service>(userConfigService);
        
        return serviceCollection;
    }
}