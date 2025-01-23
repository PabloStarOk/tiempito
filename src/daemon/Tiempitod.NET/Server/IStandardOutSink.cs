namespace Tiempitod.NET.Server;

/// <summary>
/// Defines a sink which queues messages to be sent to standard output
/// of the current connected client.
/// </summary>
public interface IStandardOutSink
{
    /// <summary>
    /// Starts the sink, allowing it to begin queuing messages.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public void Start(CancellationToken cancellationToken);

    /// <summary>
    /// Stops the sink asynchronously, ensuring all queued messages are processed.
    /// </summary>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    public Task StopAsync();
}
