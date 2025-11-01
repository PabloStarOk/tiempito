namespace Tiempito.Daemon.Domain.Notifications.Enums;

/// <summary>
/// Represents the types of notifications that can be triggered in a session.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Notification for when a session has started.
    /// </summary>
    SessionStarted,

    /// <summary>
    /// Notification for when a session interval has been completed.
    /// </summary>
    SessionIntervalCompleted,

    /// <summary>
    /// Notification for when a session has been completed.
    /// </summary>
    SessionCompleted,
}