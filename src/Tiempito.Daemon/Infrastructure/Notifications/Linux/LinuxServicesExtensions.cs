#if LINUX
using Tmds.DBus.Protocol;

namespace Tiempito.Daemon.Infrastructure.Notifications.Linux;

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
    /// <param name="services">Collection of services.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static void AddLinuxNotificationsDbus(this IServiceCollection services)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Address.Session, "Session address is null.");
        
        services.AddSingleton(_ =>
            {
                var connection = new Connection(Address.Session);
                connection.ConnectAsync().AsTask().GetAwaiter().GetResult();
                return connection;
            });
    
        services.AddSingleton(sp =>
            {
                var connection = sp.GetRequiredService<Connection>();
                return new NotificationsService(connection, NotificationsServiceName);
            });

        services.AddSingleton(sp =>
        {
            var notificationsService = sp.GetRequiredService<NotificationsService>();
            return notificationsService.CreateNotifications(NotificationsObjectPath);
        });
    }
}
#endif