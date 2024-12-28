using System.IO.Pipes;
using Tiempitod.NET.Server.Messages;

namespace Tiempitod.NET.Server;

/// <summary>
/// Defines a handler to read and write messages in a named pipe.
/// </summary>
public interface IAsyncMessageHandler
{
    /// <summary>
    /// Sends a message that contains an object.
    /// </summary>
    /// <returns></returns>
    public Task SendMessageAsync<T>(PipeStream ioStream, T obj, CancellationToken cancellationToken) where T : DaemonResponse;
    
    /// <summary>
    /// Reads a received message from a string.
    /// </summary>
    /// <returns>A string representing the message sent by the client.</returns>
    public Task<string> ReadMessageAsync(PipeStream ioStream, CancellationToken cancellationToken);
}
