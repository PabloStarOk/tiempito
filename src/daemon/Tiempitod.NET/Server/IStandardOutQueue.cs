namespace Tiempitod.NET.Server;

/// <summary>
/// Defines a writer to send messages to the
/// current connected client.
/// </summary>
public interface IStandardOutQueue
{
    /// <summary>
    /// Queues a message to be sent to the current connected client.
    /// </summary>
    /// <param name="message">The message to be queued.</param>
    public void QueueMessage(string message);
}
