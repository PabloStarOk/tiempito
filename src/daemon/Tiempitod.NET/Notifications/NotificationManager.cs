using Microsoft.Extensions.Options;
using Tiempitod.NET.Configuration;
using Tiempitod.NET.Configuration.Notifications;
using Tiempitod.NET.Configuration.User;
using Tmds.DBus.Protocol;

namespace Tiempitod.NET.Notifications;

/// <summary>
/// Concrete class to display or close notifications.
/// </summary>
public class NotificationManager : DaemonService, INotificationManager
{
    private readonly ISystemNotifier _systemNotifier;
    private readonly IUserConfigProvider _userConfigProvider;
    private Notification _baseNotification;

    public NotificationManager(
        ILogger<NotificationManager> logger,
        IOptions<NotificationConfig> notificationConfigOptions,
        IUserConfigProvider userConfigProvider,
        ISystemNotifier systemNotifier) : base(logger)
    {
        _baseNotification = new Notification(
            notificationConfigOptions.Value.AppName,
            icon: notificationConfigOptions.Value.IconPath,
            expirationTimeout: notificationConfigOptions.Value.ExpirationTimeoutMs);
        _userConfigProvider = userConfigProvider;
        _systemNotifier = systemNotifier;
    }

    protected override void OnStartService()
    {
        _baseNotification.Hints.TryAdd("sound-name", VariantValue.String("message-new-instant"));
        _baseNotification.Hints.TryAdd("category", "im");
    }

    protected override void OnStopService()
    {
        _systemNotifier.CleanUp();
    }

    public async Task NotifyAsync(string summary, string body)
    {
        if (!_userConfigProvider.UserConfig.NotificationsEnabled)
            return;
        
        _baseNotification.Summary = summary;
        _baseNotification.Body = body;
        await _systemNotifier.NotifyAsync(_baseNotification);
    }

    public async Task CloseLastNotificationAsync()
    {
        await _systemNotifier.CloseNotificationAsync();
    }
}
