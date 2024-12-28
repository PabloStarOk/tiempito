#if WINDOWS10_0_17763_0_OR_GREATER
using Microsoft.Toolkit.Uwp.Notifications;
using System.Runtime.Versioning;

namespace Tiempitod.NET.Notifications.Windows;

[SupportedOSPlatform("Windows")]
public class WindowsNotifier : ISystemNotifier
{
    private readonly ILogger<WindowsNotifier> _logger;
    private Guid _lastNotificationTag = Guid.Empty;
    
    public WindowsNotifier(ILogger<WindowsNotifier> logger)
    {
        _logger = logger;
    }
    
    public Task CloseNotificationAsync()
    {
        try
        {
            if (_lastNotificationTag != Guid.Empty)
                ToastNotificationManagerCompat.History.Remove(_lastNotificationTag.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError("Error when trying to close a Windows notification at {Time}, error: {Error}", DateTimeOffset.Now, ex);
        }
        
        return Task.CompletedTask;
    }

    public Task NotifyAsync(Notification notification)
    {
        try
        {
            _lastNotificationTag = Guid.NewGuid();

            ToastContentBuilder toastNotification = new ToastContentBuilder()
                .SetToastScenario(ToastScenario.Default)
                .AddText(notification.Summary, AdaptiveTextStyle.Header)
                .AddText(notification.Body, AdaptiveTextStyle.Body)
                .AddButton
                (
                    new ToastButton()
                        .SetContent("Accept")
                        .AddArgument("action", "dismiss")
                );
            
            toastNotification.Show
            (
                toast =>
                {
                    if (OperatingSystem.IsWindowsVersionAtLeast(10,0,10240))
                    {
                        toast.Tag = _lastNotificationTag.ToString();
                        toast.ExpirationTime = DateTimeOffset.Now.AddMilliseconds(notification.ExpirationTimeout);
                    }
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError("Error when trying to raise a Windows notification at {Time}, error: {Error}", DateTimeOffset.Now, ex);
        }
        
        return Task.CompletedTask;
    }
}
#endif
