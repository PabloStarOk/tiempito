#if LINUX
namespace Tiempito.Daemon.Infrastructure.Notifications.Linux;

/// <summary>
/// Provides notification functionality for Linux using D-Bus.
/// </summary>
internal sealed class LinuxNotifier : ILinuxNotifier
{
    private const string RequiredEnvVariable = "DBUS_SESSION_BUS_ADDRESS";

    private readonly ILogger<LinuxNotifier> _logger;
    private readonly LinuxNotificationsDbus _dbusNotificationsDbus;
    private readonly bool _isRequiredEnvVariableDefined;
    private uint _currentNotificationId;
    private uint _lastNotificationId;

    /// <summary>
    /// Initializes a new instance of the <see cref="LinuxNotifier"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging notifications.</param>
    /// <param name="dbusNotificationsDbus">The D-Bus notifications handler.</param>
    public LinuxNotifier(ILogger<LinuxNotifier> logger, LinuxNotificationsDbus dbusNotificationsDbus)
    {
        _logger = logger;
        _dbusNotificationsDbus = dbusNotificationsDbus;

        _isRequiredEnvVariableDefined = Environment.GetEnvironmentVariables().Contains(RequiredEnvVariable);
        if (!_isRequiredEnvVariableDefined)
        {
            _logger.LogError("Required \"{RequiredEnvVariable}\" environment variable for Linux is not defined, notifications will not be displayed.", RequiredEnvVariable);
        }
    }

    /// <inheritdoc/>
    public async Task NotifyAsync(LinuxNotification notification)
    {
        if (!_isRequiredEnvVariableDefined)
        {
            return;
        }

        _lastNotificationId = _currentNotificationId;
        _currentNotificationId = await _dbusNotificationsDbus.NotifyAsync(
            notification.ApplicationName,
            notification.ReplacesId,
            notification.Icon,
            notification.Summary,
            notification.Body,
            notification.Actions,
            notification.Hints,
            notification.ExpirationTimeout);
        _logger.LogDebug("Notification with ID '{NotificationId}' displayed.", _currentNotificationId);
    }

    /// <inheritdoc/>
    public async Task CloseLastAsync()
    {
        if (_lastNotificationId != _currentNotificationId)
        {
            await _dbusNotificationsDbus.CloseNotificationAsync(_lastNotificationId);
        }
    }
}
#endif