using System.IO.Pipes;
using System.Text;
using System.Text.Json;

using Tiempito.IPC.Packets.Interfaces;
using Tiempito.IPC.Packets.Objects;

namespace Tiempito.IPC.Packets;

/// <summary>
/// Defines a class to send messages and read messages which contains a header and a payload.
/// </summary>
public class PipePacketHandler : IAsyncPacketHandler
{
    private readonly Encoding _encoding;
    private readonly JsonSerializerOptions _serializerOptions;
    private const int HeaderSegmentMultiplier = 256;

    public PipePacketHandler(Encoding encoding, JsonSerializerOptions serializerOptions)
    {
        _encoding = encoding;
        _serializerOptions = serializerOptions;
    }

    /// <summary>
    /// Writes an outgoing packet.
    /// </summary>
    /// <param name="ioStream">Pipe to send the message.</param>
    /// <param name="packet">Packet to send.</param>
    /// <param name="cancellationToken">Token to stop the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="IOException">Pipe is disconnected.</exception>
    /// <exception cref="ArgumentNullException"><see cref="PipeStream"/> argument is null.</exception>
    /// <exception cref="ArgumentNullException"><see cref="object"/> argument is null.</exception>
    /// <exception cref="InvalidOperationException"><see cref="PipeStream"/> doesn't support write operations.</exception>
    public async Task WritePacketAsync(PipeStream ioStream, Packet packet, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ioStream);
        ArgumentNullException.ThrowIfNull(packet);

        if (!ioStream.IsConnected)
            throw new IOException("Pipe is disconnected");
        
        if (!ioStream.CanWrite)
            throw new NotSupportedException("Stream doesn't support write operations.");
        
        string serializedPacket = JsonSerializer.Serialize(packet, _serializerOptions);
        byte[] payload = _encoding.GetBytes(serializedPacket);
        var header = new byte[2];
        
        // Write header.
        header[0] = (byte) (payload.Length / HeaderSegmentMultiplier);
        header[1] = (byte) (payload.Length & (HeaderSegmentMultiplier - 1));
        
        // Write whole buffer.
        byte[] buffer = [..header, ..payload];
        
        await ioStream.WriteAsync(buffer, cancellationToken);
        await ioStream.FlushAsync(cancellationToken);
    }
    
    /// <summary>
    /// Reads an incoming packet.
    /// </summary>
    /// <param name="ioStream">Pipe to read the message.</param>
    /// <param name="cancellationToken">Token to stop the operation.</param>
    /// <returns>A string representing the given message from the client.</returns>
    /// <exception cref="ArgumentNullException"><see cref="PipeStream"/> argument is null.</exception>
    /// <exception cref="InvalidOperationException"><see cref="Packet"/> received packet is null</exception>
    /// <exception cref="InvalidOperationException"><see cref="PipeStream"/> doesn't support write operations.</exception>
    public async Task<Packet> ReadPacketAsync(PipeStream ioStream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ioStream);
        
        if (!ioStream.IsConnected)
            throw new IOException("Pipe is disconnected");
        
        if (!ioStream.CanRead)
            throw new InvalidOperationException("Stream doesn't support read operations.");
        
        // Read header. (Length)
        int length = ioStream.ReadByte() * 256;
        length += ioStream.ReadByte();
        
        if (length <= 0)
            return new Packet(length, string.Empty);
        
        var payload = new byte[length];

        var bytesRead = 0;
        while(bytesRead < length)
        {
            bytesRead += await ioStream.ReadAsync(payload.AsMemory(bytesRead, length - bytesRead), cancellationToken);
        }

        string dataString = _encoding.GetString(payload);
        var packet = JsonSerializer.Deserialize<Packet>(dataString, _serializerOptions);
        
        if (packet == null)
            throw new InvalidOperationException("Received packet is null.");
        
        return packet;
    }
}
