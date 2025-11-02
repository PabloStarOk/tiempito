using Tiempito.IPC.Models;

namespace Tiempito.Daemon.Application.Notifications;

/// <summary>
/// Defines a contract for writing messages to the standard output queue.
/// </summary>
public interface IStandardOutQueueWriter
{
    /// <summary>
    /// Asynchronously writes a message to the standard output queue.
    /// </summary>
    /// <param name="message">The message to write.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public ValueTask WriteAsync(Message message);
}