#if LINUX
using System.Runtime.Versioning;

using Microsoft.Extensions.Options;

using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Notifications;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Notifications.Enums;
using Tiempito.Daemon.Domain.Sessions.Enums;
using Tiempito.Daemon.Domain.Sessions.ValueObjects;

using Tmds.DBus.Protocol;

namespace Tiempito.Daemon.Infrastructure.Notifications.Linux;

/// <summary>
/// Provides notification services for Linux, including displaying notifications and playing sounds.
/// </summary>
[SupportedOSPlatform("Linux")]
internal sealed class LinuxNotificationService : INotificationService, IHostedService
{
    private readonly ILogger<LinuxNotificationService> _logger;
    private readonly IOptionsMonitor<NotificationConfig> _notificationOptions;
    private readonly IAppFilesystemPathProvider _appFilesystemPathProvider;
    private readonly IOptionsMonitor<UserConfig> _userConfigMonitor;
    private readonly ILinuxNotifier _notifier;
    private readonly ILinuxSoundPlayer _soundPlayer;
    private readonly ILinuxNotificationIconLoader _iconLoader;
    private readonly LinuxNotification _baseNotification;

    /// <summary>
    /// Initializes a new instance of the <see cref="LinuxNotificationService"/> class.
    /// </summary>
    /// <param name="logger">Logger for notification service events.</param>
    /// <param name="notificationOptions">Provides notification configuration options.</param>
    /// <param name="appFilesystemPathProvider">Provides application filesystem paths.</param>
    /// <param name="userConfigMonitor">Monitors user configuration options.</param>
    /// <param name="notifier">Handles displaying notifications on Linux.</param>
    /// <param name="soundPlayer">Plays notification sounds on Linux.</param>
    /// <param name="iconLoader">Loads notification icons for Linux notifications.</param>
    public LinuxNotificationService(
        ILogger<LinuxNotificationService> logger,
        IOptionsMonitor<NotificationConfig> notificationOptions,
        IAppFilesystemPathProvider appFilesystemPathProvider,
        IOptionsMonitor<UserConfig> userConfigMonitor,
        ILinuxNotifier notifier,
        ILinuxSoundPlayer soundPlayer,
        ILinuxNotificationIconLoader iconLoader)
    {
        _logger = logger;
        _notificationOptions = notificationOptions;
        _appFilesystemPathProvider = appFilesystemPathProvider;
        _userConfigMonitor = userConfigMonitor;
        _notifier = notifier;
        _soundPlayer = soundPlayer;
        _iconLoader = iconLoader;
        _baseNotification = new LinuxNotification(
            notificationOptions.CurrentValue.AppName,
            icon: notificationOptions.CurrentValue.IconPath,
            expirationTimeout: notificationOptions.CurrentValue.ExpirationTimeoutMs);
    }

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var appIconFilePath = _appFilesystemPathProvider.ApplicationIconPath;
        if (!Path.Exists(appIconFilePath))
        {
            return;
        }

        LinuxNotificationImageData appImgData = await _iconLoader.LoadAsync(appIconFilePath);
        _baseNotification.Hints.TryAdd("image-data", appImgData.GetVariantValue());
        _baseNotification.Hints.TryAdd("category", VariantValue.String("im"));
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask NotifyAsync(SessionState sessionState, NotificationType type)
    {
        if (!_userConfigMonitor.CurrentValue.NotificationsEnabled)
        {
            return;
        }

        string soundFileName = GetSoundFileName(type);
        (string summary, string body) = GetInformation(sessionState, type);
        
        LinuxNotification notification = _baseNotification with
        {
            Summary = summary,
            Body = body,
            AudioFilePath = Path.Combine(_appFilesystemPathProvider.AppConfigDirectoryPath, soundFileName)
        };
        
        try
        {
            await _notifier.CloseLastAsync();
            ValueTask playSoundTask = _soundPlayer.PlayAsync(notification.AudioFilePath);
            await _notifier.NotifyAsync(notification);
            await playSoundTask;
            _logger.LogDebug("Notification completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not display notification.");
        }
    }

    private string GetSoundFileName(NotificationType notificationType)
    {
        var options = _notificationOptions.CurrentValue;
        return notificationType switch
        {
            NotificationType.SessionStarted => options.SessionStartedSoundName,
            NotificationType.SessionCompleted => options.SessionFinishedSoundName,
            NotificationType.SessionIntervalCompleted => options.TimeCompletedSoundName,
            _ => throw new ArgumentOutOfRangeException(nameof(notificationType), notificationType, null)
        };
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