#if WINDOWS10_0_17763_0_OR_GREATER
using System.Runtime.Versioning;

using Microsoft.Extensions.Options;
using Microsoft.Toolkit.Uwp.Notifications;

using Tiempito.Daemon.Application.Notifications;
using Tiempito.Daemon.Application.Shared;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Notifications.Enums;
using Tiempito.Daemon.Domain.Sessions.Enums;
using Tiempito.Daemon.Domain.Sessions.ValueObjects;

namespace Tiempito.Daemon.Infrastructure.Notifications.Windows;

/// <summary>
/// Provides Windows-specific system notification functionality.
/// </summary>
[SupportedOSPlatform("Windows")]
public class WindowsNotificationService : INotificationService
{
    private readonly ILogger<WindowsNotificationService> _logger;
    private readonly IOptionsMonitor<NotificationConfig> _notificationOptions;
    private readonly IOptionsMonitor<UserConfig> _userConfigOptions;
    private readonly WindowsNotification _baseNotification;
    private Guid _lastNotificationTag = Guid.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsNotificationService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging notification events.</param>
    /// <param name="notificationOptions">The notification configuration options monitor.</param>
    /// <param name="userConfigOptions">The user configuration options monitor.</param>
    public WindowsNotificationService(
        ILogger<WindowsNotificationService> logger,
        IOptionsMonitor<NotificationConfig> notificationOptions,
        IOptionsMonitor<UserConfig> userConfigOptions)
    {
        _logger = logger;
        _notificationOptions = notificationOptions;
        _userConfigOptions = userConfigOptions;
        _baseNotification = new WindowsNotification(
            Header: string.Empty,
            Body: string.Empty,
            IconFilePath: new Uri(Paths.ApplicationIconPath),
            ExpirationTime: notificationOptions.CurrentValue.ExpirationTimeoutMs);
    }

    // TODO: Add Windows custom notification sounds.

    /// <inheritdoc/>
    public ValueTask NotifyAsync(SessionState sessionState, NotificationType type)
    {
        if (!_userConfigOptions.CurrentValue.NotificationsEnabled)
        {
            return ValueTask.CompletedTask;
        }

        _lastNotificationTag = Guid.NewGuid();
        (string header, string body) = GetInformation(sessionState, type);
        WindowsNotification notification = _baseNotification with { Header = header, Body = body, };

        try
        {
            ToastContentBuilder toastNotification = new ToastContentBuilder()
                .SetToastScenario(ToastScenario.Default)
                .AddText(notification.Header, AdaptiveTextStyle.Header)
                .AddText(notification.Body, AdaptiveTextStyle.Body)
                .AddAppLogoOverride(_baseNotification.IconFilePath)
                .AddButton(
                    new ToastButton()
                        .SetContent("Accept")
                        .AddArgument("action", "dismiss"));

            CloseLast();
            toastNotification.Show(
                toast =>
                {
                    if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240))
                    {
                        toast.Tag = _lastNotificationTag.ToString();
                        toast.ExpirationTime = DateTimeOffset.Now.AddMilliseconds(notification.ExpirationTime);
                    }
                });
            _logger.LogDebug("Notification with tag '{NotificationTag}' displayed.", _lastNotificationTag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when trying to raise a Windows notification at {Time}", DateTimeOffset.Now);
        }

        return ValueTask.CompletedTask;
    }

    private void CloseLast()
    {
        if (_lastNotificationTag == Guid.Empty)
        {
            return;
        }

        ToastNotificationManagerCompat.History.Clear();
    }

    private (string, string) GetInformation(SessionState sessionState, NotificationType notificationType)
    {
        var options = _notificationOptions.CurrentValue;
        return notificationType switch
        {
            NotificationType.SessionStarted => (options.SessionStartedSummary, options.SessionStartedBody),
            NotificationType.SessionCompleted => (options.SessionFinishedSummary, options.SessionFinishedBody),
            NotificationType.SessionIntervalCompleted => sessionState.IntervalType is SessionIntervalType.Focus
                ? (options.FocusCompletedSummary, options.FocusCompletedBody)
                : (options.BreakCompletedSummary, options.BreakCompletedBody),
            _ => throw new ArgumentOutOfRangeException(nameof(notificationType), notificationType, null)
        };
    }
}
#endif