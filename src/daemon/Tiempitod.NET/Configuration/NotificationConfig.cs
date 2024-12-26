namespace Tiempitod.NET.Configuration;

public class NotificationConfig
{
    public const string Notification = "Notification";
    
    public int ExpirationTimeoutMs { get; set; }
    public string SessionStartedSummary { get; set; } = "Session started";
    public string SessionStartedBody { get; set; } = "A new session was started.";
    public string SessionFinishedSummary { get; set; } = "Session finished";
    public string SessionFinishedBody { get; set; } = "Session finished.";
    public string FocusCompletedSummary { get; set; } = "Focus completed";
    public string FocusCompletedBody { get; set; } = "A focus time was completed.";
    public string BreakCompletedSummary { get; set; } = "Break completed";
    public string BreakCompletedBody { get; set; } = "A break time was completed.";
}
