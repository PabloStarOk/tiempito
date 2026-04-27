#if LINUX
namespace Tiempito.Daemon.Infrastructure.Notifications.Linux;

/// <summary>
/// Defines methods for sending and closing Linux desktop notifications.
/// </summary>
internal interface ILinuxNotifier
{
    /// <summary>
    /// Sends a notification to the Linux desktop environment.
    /// </summary>
    /// <param name="notification">The notification details to display.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task NotifyAsync(LinuxNotification notification);

    /// <summary>
    /// Closes the last notification that was sent.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task CloseLastAsync();
}
#endif