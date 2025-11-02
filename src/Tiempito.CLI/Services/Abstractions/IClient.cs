using Tiempito.IPC.Models;
using Tiempito.IPC.Models.Commands;

namespace Tiempito.CLI.Services.Abstractions;

/// <summary>
/// Defines a client to connect and send requests to the daemon.
/// </summary>
public interface IClient
{
    /// <summary>
    /// Sends a command to the daemon asynchronously.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task SendCommandAsync(Command command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Receives a message from the daemon asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <typeparam name="TMessage">
    /// The type of message to receive, which must inherit from <see cref="Message"/>.
    /// </typeparam>
    /// <returns>
    /// A task representing the asynchronous operation, with a nullable <see cref="Message"/> result.
    /// </returns>
    public Task<TMessage> ReceiveMessageAsync<TMessage>(CancellationToken cancellationToken = default)
        where TMessage : Message;
}
