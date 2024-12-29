using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using Tiempitod.NET.Server.Messages;

namespace Tiempitod.NET.Server;

/// <summary>
/// Defines a class to send messages and read messages which contains a header and a payload.
/// </summary>
public class PipeMessageHandler : IAsyncMessageHandler
{
    private readonly Encoding _encoding;
    private const int HeaderSegmentMultiplier = 256;
    
    public PipeMessageHandler(Encoding encoding)
    {
        _encoding = encoding;
    }

    /// <summary>
    /// Sends a message across the given pipe.
    /// </summary>
    /// <param name="ioStream">Pipe to send the message.</param>
    /// <param name="obj">Object to send in the payload.</param>
    /// <param name="cancellationToken">Token to stop the operation.</param>
    /// <typeparam name="T">Type of the object to serialize.</typeparam>
    /// <exception cref="IOException">Pipe is disconnected.</exception>
    /// <exception cref="ArgumentNullException"><see cref="PipeStream"/> argument is null.</exception>
    /// <exception cref="ArgumentNullException"><see cref="object"/> argument is null.</exception>
    /// <exception cref="InvalidOperationException"><see cref="PipeStream"/> doesn't support write operations.</exception>
    public async Task SendMessageAsync<T>(PipeStream ioStream, T obj, CancellationToken cancellationToken) where T : Response
    {
        ArgumentNullException.ThrowIfNull(ioStream);
        ArgumentNullException.ThrowIfNull(obj);

        if (!ioStream.IsConnected)
            throw new IOException("Pipe is disconnected");
        
        if (!ioStream.CanWrite)
            throw new NotSupportedException("Stream doesn't support write operations.");

        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(obj);
        byte[] header = new byte[2];
        
        // Write header.
        header[0] = (byte) (payload.Length / HeaderSegmentMultiplier);
        header[1] = (byte) (payload.Length & (HeaderSegmentMultiplier - 1));
        
        // Write whole buffer.
        byte[] buffer = [..header, ..payload];
        
        await ioStream.WriteAsync(buffer, cancellationToken);
        await ioStream.FlushAsync(cancellationToken);
    }
    
    /// <summary>
    /// Reads a given message from the client.
    /// </summary>
    /// <param name="ioStream">Pipe to send the message.</param>
    /// <param name="cancellationToken">Token to stop the operation.</param>
    /// <returns>A string representing the given message from the client.</returns>
    /// <exception cref="IOException">Pipe is disconnected.</exception>
    /// <exception cref="ArgumentNullException"><see cref="PipeStream"/> argument is null.</exception>
    /// <exception cref="InvalidOperationException"><see cref="PipeStream"/> doesn't support write operations.</exception>
    public async Task<Request> ReadMessageAsync(PipeStream ioStream, CancellationToken cancellationToken)
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
            return new Request(string.Empty, length);
        
        var payload = new byte[length];

        var bytesRead = 0;
        while(bytesRead < length)
        {
            bytesRead += await ioStream.ReadAsync(payload.AsMemory(bytesRead, length - bytesRead), cancellationToken);
        }

        string dataString = _encoding.GetString(payload);
        return new Request(dataString, length);
    }
}
