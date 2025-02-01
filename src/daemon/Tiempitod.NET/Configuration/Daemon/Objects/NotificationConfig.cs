namespace Tiempitod.NET.Configuration.Daemon.Objects;

/// <summary>
/// Represents the configuration of the notifications displayed by the daemon.
/// </summary>
public class NotificationConfig
{
    public const string Notification = "Notification";
    
    public string AppName { get; init; } = "Tiempito";
    public string IconPath { get; init; } = string.Empty;
    public int ExpirationTimeoutMs { get; init; } = 10000;
    public string SessionStartedSoundName { get; init; } = "session-alarm.wav";
    public string SessionFinishedSoundName { get; init; }  = "session-alarm.wav";
    public string TimeCompletedSoundName { get; init; } = "time-completed-alarm.wav";
    public string SessionStartedSummary { get; init; } = "Session started";
    public string SessionStartedBody { get; init; } = "A new session was started.";
    public string SessionFinishedSummary { get; init; } = "Session finished";
    public string SessionFinishedBody { get; init; } = "Session finished.";
    public string FocusCompletedSummary { get; init; } = "Focus completed";
    public string FocusCompletedBody { get; init; } = "A focus time was completed.";
    public string BreakCompletedSummary { get; init; } = "Break completed";
    public string BreakCompletedBody { get; init; } = "A break time was completed.";
}
