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
    private readonly ISystemSoundPlayer _soundPlayer;
    private uint _lastNotificationId;

    public LinuxNotifier(ISystemSoundPlayer soundPlayer)
    {
        _connection = Connection.Session;
        _dbusInterface = _connection.CreateProxy<IDBusLinuxNotification>
        (
            serviceName: "org.freedesktop.Notifications",
            path: "/org/freedesktop/Notifications"
        );
        _soundPlayer = soundPlayer;
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
        Task playSoundTask = _soundPlayer.PlayAsync(notification.AudioFilePath);
        Task<uint> notifyTask = _dbusInterface.NotifyAsync
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
        await playSoundTask;
        _lastNotificationId = await notifyTask;
    }

    /// <summary>
    /// Calls the D-Bus notification interface provided by Linux.
    /// </summary>
    public async Task CloseNotificationAsync()
    {
        Task stopSoundTask = _soundPlayer.StopAsync();
        Task closeTask = _dbusInterface.CloseNotificationAsync(_lastNotificationId);

        await stopSoundTask;
        await closeTask;
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
