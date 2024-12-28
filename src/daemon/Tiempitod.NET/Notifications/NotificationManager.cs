using Microsoft.Extensions.Options;
using Tiempitod.NET.Configuration.Notifications;
using Tmds.DBus.Protocol;

namespace Tiempitod.NET.Notifications;

/// <summary>
/// Concrete class to display or close notifications.
/// </summary>
public class NotificationManager : DaemonService, INotificationManager
{
    private readonly INotificationHandler _notificationHandler;
    private Notification _baseNotification;

    public NotificationManager(
        ILogger<NotificationManager> logger,
        IOptions<NotificationConfig> notificationConfigOptions,
        INotificationHandler notificationHandler) : base(logger)
    {
        _baseNotification = new Notification(
            notificationConfigOptions.Value.AppName,
            icon: notificationConfigOptions.Value.IconPath,
            expirationTimeout: notificationConfigOptions.Value.ExpirationTimeoutMs);
        _notificationHandler = notificationHandler;
    }

    protected override void OnStartService()
    {
        _baseNotification.Hints.TryAdd("sound-name", VariantValue.String("message-new-instant"));
        _baseNotification.Hints.TryAdd("category", "im");
    }

    protected override void OnStopService()
    {
        _notificationHandler.CleanUp();
    }

    public async Task NotifyAsync(string summary, string body)
    {
        _baseNotification.Summary = summary;
        _baseNotification.Body = body;
        await _notificationHandler.NotifyAsync(_baseNotification);
    }

    public async Task CloseLastNotificationAsync()
    {
        await _notificationHandler.CloseNotificationAsync();
    }
}
