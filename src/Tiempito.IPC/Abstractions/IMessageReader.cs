namespace Tiempito.IPC.Abstractions;

/// <summary>
/// Defines a contract for reading messages from a stream.
/// </summary>
public interface IMessageReader
{
    /// <summary>
    /// Asynchronously reads a message of type <typeparamref name="TMessage"/> from the provided stream.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to read. Must not be null.</typeparam>
    /// <param name="stream">The stream to read the message from.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous read operation. The task result contains the message of type <typeparamref name="TMessage"/>, or null if no message was read.
    /// </returns>
    Task<TMessage?> ReadAsync<TMessage>(Stream stream, CancellationToken cancellationToken = default)
        where TMessage : notnull;
}