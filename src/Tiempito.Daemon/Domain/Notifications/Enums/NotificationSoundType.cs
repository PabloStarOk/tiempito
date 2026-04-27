namespace Tiempito.Daemon.Domain.Notifications.Enums;

/// <summary>
/// Represents the types of notification sounds available in the system.
/// </summary>
public enum NotificationSoundType
{
    /// <summary>
    /// Sound played when a session starts.
    /// </summary>
    SessionStarted,

    /// <summary>
    /// Sound played when a time period is completed.
    /// </summary>
    TimeCompleted,

    /// <summary>
    /// Sound played when a session finishes.
    /// </summary>
    SessionFinished,
}
