using Microsoft.Extensions.Options;

using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Config.User;
using Tiempito.Daemon.Domain.Notifications.Enums;

using Tmds.DBus.Protocol;

namespace Tiempito.Daemon.Application.Notifications;

/// <summary>
/// Concrete class to display or close notifications.
/// </summary>
public class NotificationService : INotificationService, IHostedService
{
    private readonly ISystemNotifier _systemNotifier;
    private readonly IAppFilesystemPathProvider _appFilesystemPathProvider;
    private readonly IUserConfigService _userConfigService;
    private readonly IOptionsMonitor<NotificationConfig> _notificationConfigOptions;
#if LINUX
    private readonly ISystemAsyncIconLoader _systemAsyncIconLoader;
#endif
    private readonly string _appIconFilePath;
    private Notification _baseNotification;

    public NotificationService(
        ILogger<NotificationService> logger,
        IOptionsMonitor<NotificationConfig> notificationConfigOptions,
        IAppFilesystemPathProvider appFilesystemPathProvider,
        IUserConfigService userConfigService,
        ISystemNotifier systemNotifier
#if LINUX
        , ISystemAsyncIconLoader systemAsyncIconLoader
#endif
        )
    {
        _baseNotification = new Notification(
            notificationConfigOptions.CurrentValue.AppName,
            icon: notificationConfigOptions.CurrentValue.IconPath,
            expirationTimeout: notificationConfigOptions.CurrentValue.ExpirationTimeoutMs);
        _appFilesystemPathProvider = appFilesystemPathProvider;
        _userConfigService = userConfigService;
        _systemNotifier = systemNotifier;
        _notificationConfigOptions = notificationConfigOptions;
#if LINUX
        _systemAsyncIconLoader = systemAsyncIconLoader;
#endif
        _appIconFilePath = appFilesystemPathProvider.ApplicationIconPath;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
#if LINUX
        if (!Path.Exists(_appIconFilePath))
            return;
        NotificationImageData appImgData = await _systemAsyncIconLoader.LoadAsync(_appIconFilePath);
        _baseNotification.Hints.TryAdd("image-data", appImgData.GetVariantValue());
        _baseNotification.Hints.TryAdd("category", VariantValue.String("im"));
#elif WINDOWS10_0_17763_0_OR_GREATER
        _baseNotification.Icon = _appIconFilePath;
#endif
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _systemNotifier.CleanUp();
        return Task.CompletedTask;
    }
    
    public async Task NotifyAsync(string summary, string body, NotificationSoundType notificationSoundType)
    {
        if (!_userConfigService.UserConfig.NotificationsEnabled)
            return;
        
        _baseNotification.Summary = summary;
        _baseNotification.Body = body;
        string soundFileName = notificationSoundType switch
        {
            NotificationSoundType.SessionStarted => _notificationConfigOptions.CurrentValue.SessionStartedSoundName,
            NotificationSoundType.SessionFinished => _notificationConfigOptions.CurrentValue.SessionFinishedSoundName,
            NotificationSoundType.TimeCompleted => _notificationConfigOptions.CurrentValue.TimeCompletedSoundName,
            _ => string.Empty
        };
        _baseNotification.AudioFilePath = Path.Combine(_appFilesystemPathProvider.AppConfigDirectoryPath, soundFileName);

        await _systemNotifier.NotifyAsync(_baseNotification);
    }

    public async Task CloseLastNotificationAsync()
    {
        await _systemNotifier.CloseNotificationAsync();
    }
}
