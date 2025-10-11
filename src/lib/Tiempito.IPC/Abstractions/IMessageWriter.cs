namespace Tiempito.IPC.Abstractions;

/// <summary>
/// Defines a contract for writing messages to a stream asynchronously.
/// </summary>
public interface IMessageWriter
{
    /// <summary>
    /// Writes the specified message to the provided stream asynchronously.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to write. Must not be null.</typeparam>
    /// <param name="stream">The stream to which the message will be written.</param>
    /// <param name="message">The message to write.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task WriteAsync<TMessage>(
        Stream stream,
        TMessage message,
        CancellationToken cancellationToken = default)
        where TMessage : notnull;
}