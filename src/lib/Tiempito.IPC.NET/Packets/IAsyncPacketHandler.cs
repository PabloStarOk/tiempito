using System.IO.Pipes;

namespace Tiempito.IPC.NET.Packets;

/// <summary>
/// Defines a handler to read and write messages in a named pipe.
/// </summary>
public interface IAsyncPacketHandler
{
    /// <summary>
    /// Writes an outgoing packet.
    /// </summary>
    /// <param name="ioStream">Pipe to send the message.</param>
    /// <param name="packet">Packet to send.</param>
    /// <param name="cancellationToken">Token to stop the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task WritePacketAsync(PipeStream ioStream, Packet packet, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reads an incoming packet.
    /// </summary>
    /// <param name="ioStream">Pipe to read the message.</param>
    /// <param name="cancellationToken">Token to stop the operation.</param>
    /// <returns>A string representing the given message from the client.</returns>
    public Task<Packet> ReadPacketAsync(PipeStream ioStream, CancellationToken cancellationToken = default);
}
