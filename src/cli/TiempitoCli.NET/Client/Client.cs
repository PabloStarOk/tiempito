using System.IO.Pipes;
using Tiempito.IPC.NET.Messages;
using Tiempito.IPC.NET.Packets;

namespace TiempitoCli.NET.Client;

public class Client
{
    private readonly NamedPipeClientStream _pipeClient;
    private readonly IAsyncPacketHandler _packetHandler;
    private readonly IPacketSerializer _packetSerializer;
    private readonly IPacketDeserializer _packetDeserializer;
    private readonly int _connectionTimeout;
    
    public Client(
        NamedPipeClientStream pipeClient,
        IAsyncPacketHandler packetHandler,
        IPacketSerializer packetSerializer,
        IPacketDeserializer packetDeserializer,
        int connectionTimeout)
    {
        _pipeClient = pipeClient;
        _packetHandler = packetHandler;
        _packetDeserializer = packetDeserializer;
        _connectionTimeout = connectionTimeout;
    }
    
    public async Task SendRequestAsync(Request request)
    {
        try
        {
            await _pipeClient.ConnectAsync(_connectionTimeout);
        }
        catch (TimeoutException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Daemon is not running.");
            Console.ForegroundColor = ConsoleColor.White;
            return;
        }

        if (!_pipeClient.IsConnected)
            return;

        Packet packet = _packetSerializer.Serialize(request);
        await _packetHandler.WritePacketAsync(_pipeClient, packet);
    }

    public async Task<Response> ReceiveResponseAsync()
    {
        if (!_pipeClient.IsConnected)
            throw new InvalidOperationException("Named pipe is not connected.");

        Packet? packet = await _packetHandler.ReadPacketAsync(_pipeClient);

        if (packet == null)
            throw new InvalidOperationException("Response not recognized.");
        
        return _packetDeserializer.Deserialize<Response>(packet);
    }
}
