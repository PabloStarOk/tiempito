using Tiempito.IPC.Models;

namespace Tiempito.CLI.Services.Abstractions;

/// <summary>
/// Defines a client to connect and send requests to the daemon.
/// </summary>
public interface IClient
{
    /// <summary>
    /// Sends a message to the daemon asynchronously.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task SendMessageAsync(Message message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Receives a message from the daemon asynchronously.
    /// </summary>
    /// <param name="useTimeout">Whether to use a timeout when receiving the message.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <typeparam name="TMessage">
    /// The type of message to receive, which must inherit from <see cref="Message"/>.
    /// </typeparam>
    /// <returns>
    /// A task representing the asynchronous operation, with a nullable <see cref="Message"/> result.
    /// </returns>
    public Task<TMessage> ReceiveMessageAsync<TMessage>(bool useTimeout, CancellationToken cancellationToken = default)
        where TMessage : Message;
}
