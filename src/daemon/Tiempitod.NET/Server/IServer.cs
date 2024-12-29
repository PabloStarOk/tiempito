using Tiempitod.NET.Server.Messages;

namespace Tiempitod.NET.Server;

/// <summary>
/// Defines a server to receive requests and send responses.
/// </summary>
public interface IServer
{
    public event EventHandler<Request> RequestReceived;

    /// <summary>
    /// Sends a response to the current connected client.
    /// </summary>
    /// <returns>A Task representing the operation.</returns>
    public Task SendResponseAsync(Response response);
}
