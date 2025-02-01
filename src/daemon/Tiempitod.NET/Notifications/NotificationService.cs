using Microsoft.Extensions.Options;

using Tiempitod.NET.Common;
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
public class NotificationService : Service, INotificationService
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
        ) : base(logger)
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

    protected async override Task<bool> OnStartServiceAsync()
    {
#if LINUX
        if (!Path.Exists(_appIconFilePath))
            return false;
        NotificationImageData appImgData = await _systemAsyncIconLoader.LoadAsync(_appIconFilePath);
        _baseNotification.Hints.TryAdd("image-data", appImgData.GetVariantValue());
        _baseNotification.Hints.TryAdd("category", VariantValue.String("im"));
#elif WINDOWS10_0_17763_0_OR_GREATER
        _baseNotification.Icon = _appIconFilePath;
#endif
        return true;
    }

    protected override Task<bool> OnStopServiceAsync()
    {
        _systemNotifier.CleanUp();
        return Task.FromResult(true);
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
