using Tiempito.Daemon.Notifications.Objects;

namespace Tiempito.Daemon.Notifications.Interfaces;

/// <summary>
/// Defines a notifier to display popup notifications in an operating system.
/// </summary>
public interface ISystemNotifier
{
    /// <summary>
    /// Allows to clean up the managed and unmanaged resources of the notifier if needed.
    /// </summary>
    public void CleanUp() { }
    
    /// <summary>
    /// Creates a notification in the desktop environment
    /// </summary>
    /// <param name="notification">Notification to display.</param>
    /// <returns>A task representing the async operation.</returns>
    public Task NotifyAsync(Notification notification);
    
    /// <summary>
    /// Close the last displayed notification
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    public Task CloseNotificationAsync();
}
