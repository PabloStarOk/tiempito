using Tmds.DBus.Protocol;

namespace Tiempitod.NET.Notifications;

/// <summary>
/// Concrete class to display or close notifications.
/// </summary>
public class NotificationManager : DaemonService, INotificationManager
{
    private readonly INotificationHandler _notificationHandler;
    // TODO: Replace with configuration.
    private Notification _baseNotification = new("Tiempito", string.Empty, string.Empty, expirationTimeout: 10000);

    public NotificationManager(ILogger<NotificationManager> logger, INotificationHandler notificationHandler) : base(logger)
    {
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
