namespace Tiempito.Daemon.Application.Notifications;

/// <summary>
/// Represents the configuration of the notifications displayed by the daemon.
/// </summary>
public class NotificationConfig
{
    /// <summary>
    /// The configuration section name for notifications.
    /// </summary>
    public const string Notification = "Notification";

    /// <summary>
    /// Gets the name of the application displayed in notifications.
    /// </summary>
    public string AppName { get; init; } = "Tiempito";

    /// <summary>
    /// Gets the path to the icon used in notifications.
    /// </summary>
    public string IconPath { get; init; } = string.Empty;

    /// <summary>
    /// Gets the expiration timeout for notifications in milliseconds.
    /// </summary>
    public int ExpirationTimeoutMs { get; init; } = 10000;

    /// <summary>
    /// Gets the sound file name played when a session starts.
    /// </summary>
    public string SessionStartedSoundName { get; init; } = "session-alarm.wav";

    /// <summary>
    /// Gets the sound file name played when a session finishes.
    /// </summary>
    public string SessionFinishedSoundName { get; init; } = "session-alarm.wav";

    /// <summary>
    /// Gets the sound file name played when a time period is completed.
    /// </summary>
    public string TimeCompletedSoundName { get; init; } = "time-completed-alarm.wav";

    /// <summary>
    /// Gets the summary text for the session started notification.
    /// </summary>
    public string SessionStartedSummary { get; init; } = "Session started";

    /// <summary>
    /// Gets the body text for the session started notification.
    /// </summary>
    public string SessionStartedBody { get; init; } = "A new session was started.";

    /// <summary>
    /// Gets the summary text for the session finished notification.
    /// </summary>
    public string SessionFinishedSummary { get; init; } = "Session finished";

    /// <summary>
    /// Gets the body text for the session finished notification.
    /// </summary>
    public string SessionFinishedBody { get; init; } = "Session finished.";

    /// <summary>
    /// Gets the summary text for the focus completed notification.
    /// </summary>
    public string FocusCompletedSummary { get; init; } = "Focus completed";

    /// <summary>
    /// Gets the body text for the focus completed notification.
    /// </summary>
    public string FocusCompletedBody { get; init; } = "A focus time was completed.";

    /// <summary>
    /// Gets the summary text for the break completed notification.
    /// </summary>
    public string BreakCompletedSummary { get; init; } = "Break completed";

    /// <summary>
    /// Gets the body text for the break completed notification.
    /// </summary>
    public string BreakCompletedBody { get; init; } = "A break time was completed.";
}
