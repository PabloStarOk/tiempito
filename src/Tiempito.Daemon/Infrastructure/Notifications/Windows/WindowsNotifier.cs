#if WINDOWS10_0_17763_0_OR_GREATER
using System.Runtime.Versioning;

using Microsoft.Toolkit.Uwp.Notifications;

using Tiempito.Daemon.Application.Notifications;

namespace Tiempito.Daemon.Infrastructure.Notifications.Windows;

/// <summary>
/// Provides Windows-specific system notification functionality.
/// </summary>
[SupportedOSPlatform("Windows")]
public class WindowsNotifier : ISystemNotifier
{
    private readonly ILogger<WindowsNotifier> _logger;
    private Guid _lastNotificationTag = Guid.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsNotifier"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging notification events.</param>
    public WindowsNotifier(ILogger<WindowsNotifier> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task CloseNotificationAsync()
    {
        try
        {
            if (_lastNotificationTag != Guid.Empty)
            {
                ToastNotificationManagerCompat.History.Clear();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error when trying to close a Windows notification at {Time}, error: {Error}", DateTimeOffset.Now, ex);
        }

        return Task.CompletedTask;
    }

    // TODO: Add Windows custom notification sounds.

    /// <inheritdoc/>
    public Task NotifyAsync(Notification notification)
    {
        try
        {
            _lastNotificationTag = Guid.NewGuid();

            var iconUri = new Uri(notification.Icon);

            ToastContentBuilder toastNotification = new ToastContentBuilder()
                .SetToastScenario(ToastScenario.Default)
                .AddText(notification.Summary, AdaptiveTextStyle.Header)
                .AddText(notification.Body, AdaptiveTextStyle.Body)
                .AddAppLogoOverride(iconUri)
                .AddButton(
                    new ToastButton()
                        .SetContent("Accept")
                        .AddArgument("action", "dismiss"));

            toastNotification.Show(
                toast =>
                {
                    if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240))
                    {
                        toast.Tag = _lastNotificationTag.ToString();
                        toast.ExpirationTime = DateTimeOffset.Now.AddMilliseconds(notification.ExpirationTimeout);
                    }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when trying to raise a Windows notification at {Time}", DateTimeOffset.Now);
        }

        return Task.CompletedTask;
    }
}
#endif
