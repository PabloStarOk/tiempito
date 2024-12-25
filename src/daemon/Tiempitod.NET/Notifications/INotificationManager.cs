namespace Tiempitod.NET.Notifications;

/// <summary>
/// Defines a manager of notifications.
/// </summary>
public interface INotificationManager
{
    /// <summary>
    /// Displays a notification in the current operating system.
    /// </summary>
    /// <param name="summary">Summary of the notification.</param>
    /// <param name="body">Body of the notification.</param>
    /// <returns>A task representing the operation.</returns>
    public Task NotifyAsync(string summary, string body);
    
    /// <summary>
    /// Closes the last notification displayed.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    public Task CloseLastNotificationAsync();
}
