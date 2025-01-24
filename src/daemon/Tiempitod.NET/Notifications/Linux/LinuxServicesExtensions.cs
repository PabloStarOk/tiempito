#if LINUX
using Tmds.DBus.Protocol;

namespace Tiempitod.NET.Notifications.Linux;

/// <summary>
/// Extensions for linux services.
/// </summary>
public static class LinuxServicesExtensions
{
    private const string NotificationsServiceName = "org.freedesktop.Notifications";
    private const string NotificationsObjectPath = "/org/freedesktop/Notifications";
    
    /// <summary>
    /// Add the linux notification dbus service for org.freedesktop.Notifications implementation.
    /// </summary>
    /// <param name="serviceCollection">Collection of services.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddLinuxNotificationsDbus(this IServiceCollection serviceCollection)
    {
        string? sessionAddress = Address.Session;
        
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionAddress, "Session address is null.");
    
        var connection = new Connection(sessionAddress);
        connection.ConnectAsync();
    
        var service = new NotificationsService(connection, NotificationsServiceName);
        LinuxNotificationsDbus dbusNotificationsDbus = service.CreateNotifications(NotificationsObjectPath);
        
        return serviceCollection.AddSingleton(dbusNotificationsDbus);
    }
}
#endif