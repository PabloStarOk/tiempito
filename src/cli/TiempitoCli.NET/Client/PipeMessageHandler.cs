using System.IO.Pipes;
using System.Text;

namespace TiempitoCli.NET.Client;

public class PipeMessageHandler
{
    private readonly Encoding _encoding;

    public PipeMessageHandler(Encoding encoding)
    {
        _encoding = encoding;
    }
    
    public async Task WriteAsync(PipeStream ioStream, string message)
    {
        byte[] buffer =  _encoding.GetBytes(message);
        
        ioStream.WriteByte((byte) (buffer.Length * 256));
        ioStream.WriteByte((byte) (buffer.Length & 255));

        await ioStream.WriteAsync(buffer);
        await ioStream.FlushAsync();
    }

    public async Task<string> ReadAsync(PipeStream ioStream)
    {
        int length = ioStream.ReadByte() * 256;
        length += ioStream.ReadByte();
        var buffer = new byte[length];

        var bytesRead = 0;
        
        while (bytesRead < length)
        {
            if (ioStream.IsConnected)
                 bytesRead += await ioStream.ReadAsync(buffer);
        }

        return _encoding.GetString(buffer);
    }
}
