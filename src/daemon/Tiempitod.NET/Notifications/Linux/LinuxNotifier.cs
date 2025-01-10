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
    private const string RequiredEnvVariable = "DBUS_SESSION_BUS_ADDRESS";
    
    private readonly ILogger<LinuxNotifier> _logger;
    private readonly Connection _connection;
    private readonly IDBusLinuxNotification _dbusInterface;
    private readonly ISystemSoundPlayer _soundPlayer;
    private uint _lastNotificationId;
    private readonly bool _isRequiredEnvVariableDefined;

    public LinuxNotifier(ILogger<LinuxNotifier> logger, ISystemSoundPlayer soundPlayer)
    {
        _logger = logger;
        _connection = Connection.Session;
        _dbusInterface = _connection.CreateProxy<IDBusLinuxNotification>
        (
            serviceName: "org.freedesktop.Notifications",
            path: "/org/freedesktop/Notifications"
        );
        _soundPlayer = soundPlayer;

        _isRequiredEnvVariableDefined = Environment.GetEnvironmentVariables().Contains(RequiredEnvVariable);
        if (!_isRequiredEnvVariableDefined)
            _logger.LogError("Required \"{RequiredEnvVariable}\" environment variable for Linux is not defined, notifications will not be displayed.", RequiredEnvVariable);
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
        try
        {
            Task playSoundTask = _soundPlayer.PlayAsync(notification.AudioFilePath);

            if (_isRequiredEnvVariableDefined)
            {
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
                _lastNotificationId = await notifyTask;
            }

            await playSoundTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't display notification.");
        }
    }

    /// <summary>
    /// Calls the D-Bus notification interface provided by Linux.
    /// </summary>
    public async Task CloseNotificationAsync()
    {
        try
        {
            Task stopSoundTask = _soundPlayer.StopAsync();
            Task closeTask = _dbusInterface.CloseNotificationAsync(_lastNotificationId);

            await stopSoundTask;
            await closeTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't close the notification.");
        }
    }

    public void Dispose()
    {
        Dispose(isDisposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool isDisposing)
    {
        if (!isDisposing)
            return;

        _connection.Dispose();
        _soundPlayer.Dispose();
    }

    ~LinuxNotifier()
    {
        Dispose(isDisposing: false);
    }
}
#endif
