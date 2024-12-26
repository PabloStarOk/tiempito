namespace Tiempitod.NET.Configuration;

public class NotificationConfig
{
    public const string Notification = "Notification";

    public string AppName { get; init; } = "Tiempito";
    public string IconPath { get; init; } = string.Empty;
    public int ExpirationTimeoutMs { get; init; } = 10000;
    public string SessionStartedSummary { get; init; } = "Session started";
    public string SessionStartedBody { get; init; } = "A new session was started.";
    public string SessionFinishedSummary { get; init; } = "Session finished";
    public string SessionFinishedBody { get; init; } = "Session finished.";
    public string FocusCompletedSummary { get; init; } = "Focus completed";
    public string FocusCompletedBody { get; init; } = "A focus time was completed.";
    public string BreakCompletedSummary { get; init; } = "Break completed";
    public string BreakCompletedBody { get; init; } = "A break time was completed.";
}
