namespace Tiempitod.NET.Notifications;

/// <summary>
/// Defines a manager of notifications.
/// </summary>
public interface INotificationManager
{
    /// <summary>
    /// Notifies that a session was started.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    public Task NotifySessionStartedAsync();

    /// <summary>
    /// Notifies that a session was finished.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    public Task NotifySessionFinishedAsync();

    /// <summary>
    /// Notifies that a focus time was completed.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    public Task NotifyFocusTimeCompletedAsync();

    /// <summary>
    /// Notifies that a break time was completed.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    public Task NotifyBreakTimeCompletedAsync();
}
