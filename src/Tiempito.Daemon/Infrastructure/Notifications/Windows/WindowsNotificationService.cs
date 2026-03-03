#if WINDOWS10_0_17763_0_OR_GREATER
using System.Media;
using System.Runtime.Versioning;
using Microsoft.Extensions.Options;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using Tiempito.Daemon.Application.Notifications;
using Tiempito.Daemon.Application.Shared;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Notifications.Enums;
using Tiempito.Daemon.Domain.Sessions.Enums;
using Tiempito.Daemon.Domain.Sessions.ValueObjects;
using Windows.UI.Notifications;

namespace Tiempito.Daemon.Infrastructure.Notifications.Windows;

/// <summary>
/// Provides Windows-specific system notification functionality.
/// </summary>
[SupportedOSPlatform("Windows10.0.10240.0")]
public sealed class WindowsNotificationService : INotificationService, IHostedService, IDisposable
{
    private const string AppName = "Tiempito";
    private const string AppId = "PabloStarOk.Tiempito.Daemon";
    private const string RegistrySubKeyPath = $@"Software\Classes\AppUserModelId\{AppId}";

    private readonly ILogger<WindowsNotificationService> _logger;
    private readonly IOptionsMonitor<NotificationConfig> _notificationOptions;
    private readonly IOptionsMonitor<UserConfig> _userConfigOptions;
    private readonly WindowsNotification _baseNotification;
    private readonly ToastNotifier _notifier;
    private readonly ToastButton _dismissButton;
    private readonly SoundPlayer _sessionStartedSound;
    private readonly SoundPlayer _intervalCompletedSound;
    private readonly SoundPlayer _sessionCompletedSound;
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
        _notifier = ToastNotificationManager.CreateToastNotifier(AppId);
        _dismissButton = new ToastButton().SetContent("Accept").AddArgument("action", "dismiss");
        _sessionStartedSound = new SoundPlayer(
            Path.Combine(Paths.DaemonConfigDirectoryPath, _notificationOptions.CurrentValue.SessionStartedSoundName));
        _sessionCompletedSound = new SoundPlayer(
            Path.Combine(Paths.DaemonConfigDirectoryPath, _notificationOptions.CurrentValue.SessionFinishedSoundName));
        _intervalCompletedSound = new SoundPlayer(
            Path.Combine(Paths.DaemonConfigDirectoryPath, _notificationOptions.CurrentValue.TimeCompletedSoundName));
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        RegisterAppId();
        _sessionStartedSound.LoadAsync();
        _intervalCompletedSound.LoadAsync();
        _sessionCompletedSound.LoadAsync();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public ValueTask NotifyAsync(SessionState sessionState, NotificationType type)
    {
        if (!_userConfigOptions.CurrentValue.NotificationsEnabled)
        {
            return ValueTask.CompletedTask;
        }

        _lastNotificationTag = Guid.NewGuid();
        (string header, string body, SoundPlayer soundPlayer) = GetInformation(sessionState, type);
        WindowsNotification notification = _baseNotification with { Header = header, Body = body, };
        var toastAudio = new ToastAudio
        {
            Silent = true,
        };
        var notificationBuilder = new ToastContentBuilder()
            .SetToastScenario(ToastScenario.Default)
            .AddAudio(toastAudio)
            .AddText(notification.Header, AdaptiveTextStyle.Header)
            .AddText(notification.Body, AdaptiveTextStyle.Body)
            .AddButton(_dismissButton);
        var toastNotification = new ToastNotification(notificationBuilder.GetXml())
        {
            Tag = _lastNotificationTag.ToString(),
        };

        try
        {
            _notifier.Show(toastNotification);
            soundPlayer.Play();
            _logger.LogDebug("Notification with tag '{NotificationTag}' displayed.", _lastNotificationTag);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Could not play audio '{AudioFileName}' because it was not found", soundPlayer.SoundLocation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when trying to raise a Windows notification at {Time}", DateTimeOffset.Now);
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _sessionStartedSound.Stop();
        _intervalCompletedSound.Stop();
        _sessionCompletedSound.Stop();
        UnregisterAppId();
        _sessionStartedSound.Dispose();
        _intervalCompletedSound.Dispose();
        _sessionCompletedSound.Dispose();
    }

    private (string, string, SoundPlayer) GetInformation(SessionState sessionState, NotificationType notificationType)
    {
        var options = _notificationOptions.CurrentValue;
        return notificationType switch
        {
            NotificationType.SessionStarted =>
                (options.SessionStartedSummary, options.SessionStartedBody, _sessionStartedSound),
            NotificationType.SessionCompleted =>
                (options.SessionFinishedSummary, options.SessionFinishedBody, _sessionCompletedSound),
            NotificationType.SessionIntervalCompleted => sessionState.IntervalType is SessionIntervalType.Focus
                ? (options.FocusCompletedSummary, options.FocusCompletedBody, _intervalCompletedSound)
                : (options.BreakCompletedSummary, options.BreakCompletedBody, _intervalCompletedSound),
            _ => throw new ArgumentOutOfRangeException(nameof(notificationType), notificationType, null)
        };
    }

    private void RegisterAppId()
    {
        using var key = Registry.CurrentUser.CreateSubKey(RegistrySubKeyPath);
        key.SetValue("DisplayName", AppName);
        key.SetValue("IconUri", Paths.ApplicationIconPath);
        _logger.LogDebug("Application registry key added at {RegistrySubKeyPath}.", RegistrySubKeyPath);
    }

    private void UnregisterAppId()
    {
        if (Registry.CurrentUser.OpenSubKey(RegistrySubKeyPath) is null)
        {
            return;
        }

        Registry.CurrentUser.DeleteSubKey(RegistrySubKeyPath);
        _logger.LogDebug("Application registry key removed from {RegistrySubKeyPath}.", RegistrySubKeyPath);
    }
}
#endif