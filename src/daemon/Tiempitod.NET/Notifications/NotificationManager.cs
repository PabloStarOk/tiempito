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

    public async Task CloseLastNotificationAsync()
    {
        await _notificationHandler.CloseNotificationAsync();
    }
    
    public async Task NotifySessionStartedAsync()
    {
        _baseNotification.Summary = "Session started \u23f3";
        _baseNotification.Body = "A new Tiempito session started.";
        await _notificationHandler.NotifyAsync(_baseNotification);
    }
    
    public async Task NotifySessionFinishedAsync()
    {
        _baseNotification.Summary = "Session finished \ud83c\udfc6";
        _baseNotification.Body = "Tiempito session finished.";
        await _notificationHandler.NotifyAsync(_baseNotification);
    }
    
    public async Task NotifyFocusTimeCompletedAsync()
    {
        _baseNotification.Summary = "Focus completed \ud83c\udfaf";
        _baseNotification.Body = "A focus time was completed.";
        await _notificationHandler.NotifyAsync(_baseNotification);
    }
    
    public async Task NotifyBreakTimeCompletedAsync()
    {
        _baseNotification.Summary = "Break completed \ud83d\ude34";
        _baseNotification.Body = "A Break time was completed.";
        await _notificationHandler.NotifyAsync(_baseNotification);
    }
}
