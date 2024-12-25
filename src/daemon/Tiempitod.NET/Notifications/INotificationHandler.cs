namespace Tiempitod.NET.Notifications;

/// <summary>
/// Defines a handler to show and close notifications.
/// </summary>
public interface INotificationHandler : IDisposable
{
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
