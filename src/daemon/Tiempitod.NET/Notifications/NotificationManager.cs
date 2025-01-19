using Microsoft.Extensions.Options;
using Tiempitod.NET.Configuration.AppFilesystem;
using Tiempitod.NET.Configuration.Notifications;
using Tiempitod.NET.Configuration.User;
#if LINUX
using Tmds.DBus.Protocol;
#endif

namespace Tiempitod.NET.Notifications;

/// <summary>
/// Concrete class to display or close notifications.
/// </summary>
public class NotificationManager : DaemonService, INotificationManager
{
    private readonly ISystemNotifier _systemNotifier;
    private readonly IAppFilesystemPathProvider _appFilesystemPathProvider;
    private readonly IUserConfigProvider _userConfigProvider;
    private readonly IOptionsMonitor<NotificationConfig> _notificationConfigOptions;
#if LINUX
    private readonly ISystemAsyncIconLoader _systemAsyncIconLoader;
#endif
    private readonly string _appIconFilePath;
    private Notification _baseNotification;

    public NotificationManager(
        ILogger<NotificationManager> logger,
        IOptionsMonitor<NotificationConfig> notificationConfigOptions,
        IAppFilesystemPathProvider appFilesystemPathProvider,
        IUserConfigProvider userConfigProvider,
        ISystemNotifier systemNotifier
#if LINUX
        , ISystemAsyncIconLoader systemAsyncIconLoader
#endif
        ) : base(logger)
    {
        _baseNotification = new Notification(
            notificationConfigOptions.CurrentValue.AppName,
            icon: notificationConfigOptions.CurrentValue.IconPath,
            expirationTimeout: notificationConfigOptions.CurrentValue.ExpirationTimeoutMs);
        _appFilesystemPathProvider = appFilesystemPathProvider;
        _userConfigProvider = userConfigProvider;
        _systemNotifier = systemNotifier;
        _notificationConfigOptions = notificationConfigOptions;
#if LINUX
        _systemAsyncIconLoader = systemAsyncIconLoader;
#endif
        _appIconFilePath = appFilesystemPathProvider.ApplicationIconPath;
    }

    protected override void OnStartService()
    {
#if LINUX
        Task.Run
        (
            async () =>
            {
                try
                {
                    if (!Path.Exists(_appIconFilePath))
                        return;
                    NotificationImageData appImgData = await _systemAsyncIconLoader.LoadAsync(_appIconFilePath);
                    _baseNotification.Hints.TryAdd("image-data", appImgData.GetVariantValue());
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Application's icon couldn't be loaded.");
                }
            }
        );
        
        _baseNotification.Hints.TryAdd("category", VariantValue.String("im"));
#elif WINDOWS10_0_17763_0_OR_GREATER
        _baseNotification.Icon = _appIconFilePath;
#endif
    }

    protected override void OnStopService()
    {
        _systemNotifier.CleanUp();
    }
    
    public async Task NotifyAsync(string summary, string body, NotificationSoundType notificationSoundType)
    {
        if (!_userConfigProvider.UserConfig.NotificationsEnabled)
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
