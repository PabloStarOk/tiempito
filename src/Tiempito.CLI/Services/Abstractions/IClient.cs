using Tiempito.IPC.Models;

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
    /// Receives a response from the daemon asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with a nullable <see cref="Response"/> result.
    /// </returns>
    public Task<Response?> ReceiveResponseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a line of text asynchronously from the standard input pipe.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with the read string result.
    /// </returns>
    public Task<string> ReadPipeStdInAsync(CancellationToken cancellationToken = default);
}
