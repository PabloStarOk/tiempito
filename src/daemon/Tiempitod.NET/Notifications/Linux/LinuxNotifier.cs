#if LINUX
using System.Runtime.Versioning;
using Tmds.DBus;

namespace Tiempitod.NET.Notifications.Linux;

/// <summary>
/// Notification handler for Linux-based operating systems
/// </summary>
[SupportedOSPlatform("Linux")]
public class LinuxNotifier : ISystemNotifier, IDisposable
{
    private readonly Connection _connection;
    private readonly IDBusLinuxNotification _dbusInterface;
    private uint _lastNotificationId;

    public LinuxNotifier()
    {
        _connection = Connection.Session;
        _dbusInterface = _connection.CreateProxy<IDBusLinuxNotification>
        (
            serviceName: "org.freedesktop.Notifications",
            path: "/org/freedesktop/Notifications"
        );
    }

    public void CleanUp()
    {
        Dispose();
    }

    /// <summary>
    /// Calls the D-Bus notification interface provided by Linux.
    /// </summary>
    /// <param name="notification">Notification to display.</param>
    public async Task NotifyAsync(Notification notification)
    {
        _lastNotificationId = await _dbusInterface.NotifyAsync
        (
            notification.ApplicationName,
            notification.ReplacesId,
            notification.Icon,
            notification.Summary,
            notification.Body,
            notification.Actions,
            notification.Hints,
            notification.ExpirationTimeout
        );
    }

    /// <summary>
    /// Calls the D-Bus notification interface provided by Linux.
    /// </summary>
    public async Task CloseNotificationAsync()
    {
        await _dbusInterface.CloseNotificationAsync(_lastNotificationId);
    }

    public void Dispose()
    {
        Dispose(isDisposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool isDisposing)
    {
        if (!isDisposing)
            return;

        _connection.Dispose();
    }

    ~LinuxNotifier()
    {
        Dispose(isDisposing: false);
    }
}
#endif
