using Tiempito.IPC.NET.Messages;

namespace Tiempito.CLI.NET.Client;

/// <summary>
/// Defines a client to connect and send requests to the daemon.
/// </summary>
public interface IClient
{
    /// <summary>
    /// Sends a request to the daemon.
    /// </summary>
    /// <param name="request">Request to send.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task SendRequestAsync(Request request);
    
    /// <summary>
    /// Receives a response from the daemon.
    /// </summary>
    /// <returns>A task with the response from the daemon.</returns>
    public Task<Response> ReceiveResponseAsync();
}
