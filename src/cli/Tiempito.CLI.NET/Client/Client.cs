using System.IO.Pipes;
using System.Text;
using Tiempito.IPC.NET.Messages;
using Tiempito.IPC.NET.Packets;

namespace Tiempito.CLI.NET.Client;

public class Client : IClient
{
    private readonly NamedPipeClientStream _pipeClient;
    private readonly IAsyncPacketHandler _packetHandler;
    private readonly IPacketSerializer _packetSerializer;
    private readonly IPacketDeserializer _packetDeserializer;
    private const int ConnectionTimeout = 3000;
    
    public Client(
        NamedPipeClientStream pipeClient,
        IAsyncPacketHandler packetHandler,
        IPacketSerializer packetSerializer,
        IPacketDeserializer packetDeserializer)
    {
        _pipeClient = pipeClient;
        _packetHandler = packetHandler;
        _packetSerializer = packetSerializer;
        _packetDeserializer = packetDeserializer;
    }
    
    public async Task SendRequestAsync(Request request)
    {
        try
        {
            await _pipeClient.ConnectAsync(ConnectionTimeout);
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

        Packet outgoingPacket = _packetSerializer.Serialize(request);
        await _packetHandler.WritePacketAsync(_pipeClient, outgoingPacket);
    }

    public async Task<Response> ReceiveResponseAsync()
    {
        if (!_pipeClient.IsConnected)
            throw new InvalidOperationException("Named pipe is not connected.");

        Packet? incomingPacket = await _packetHandler.ReadPacketAsync(_pipeClient);

        if (incomingPacket == null)
            throw new InvalidOperationException("Response not recognized.");
        
        return _packetDeserializer.Deserialize<Response>(incomingPacket);
    }
}
