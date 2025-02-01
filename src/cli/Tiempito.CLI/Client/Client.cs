using System.IO.Pipes;
using Tiempito.IPC.NET.Messages;
using Tiempito.IPC.NET.Packets;

namespace Tiempito.CLI.Client;

/// <summary>
/// Client that sends requests to the daemon and receive responses from the daemon.
/// </summary>
public class Client : IClient
{
    private readonly NamedPipeClientStream _pipeClient;
    private readonly IAsyncPacketHandler _packetHandler;
    private readonly IPacketSerializer _packetSerializer;
    private readonly IPacketDeserializer _packetDeserializer;
    private readonly TextReader _pipeStandardIn;
    private const int ConnectionTimeout = 3000;
    
    /// <summary>
    /// Instantiates a <see cref="Client"/>. 
    /// </summary>
    /// <param name="pipeClient">Named pipe to connect to the daemon.</param>
    /// <param name="packetHandler">Handler of packets.</param>
    /// <param name="packetSerializer">Serializer of packets.</param>
    /// <param name="packetDeserializer">Deserializer of packets.</param>
    /// <param name="pipeStandardIn">Standard input of the named pipe.</param>
    public Client(
        NamedPipeClientStream pipeClient,
        IAsyncPacketHandler packetHandler,
        IPacketSerializer packetSerializer,
        IPacketDeserializer packetDeserializer,
        TextReader pipeStandardIn)
    {
        _pipeClient = pipeClient;
        _packetHandler = packetHandler;
        _packetSerializer = packetSerializer;
        _packetDeserializer = packetDeserializer;
        _pipeStandardIn = pipeStandardIn;
    }
    
    /// <summary>
    /// Sends a request to the daemon.
    /// </summary>
    /// <param name="request">Request to send to the daemon.</param>
    public async Task SendRequestAsync(Request request)
    {
        if (!_pipeClient.IsConnected)
            await _pipeClient.ConnectAsync(ConnectionTimeout);

        Packet outgoingPacket = _packetSerializer.Serialize(request);
        await _packetHandler.WritePacketAsync(_pipeClient, outgoingPacket);
    }

    /// <summary>
    /// Receives a response from the daemon.
    /// </summary>
    /// <returns>The response of the daemon.</returns>
    /// <exception cref="InvalidOperationException">If the incoming packet is null.</exception>
    public async Task<Response> ReceiveResponseAsync()
    {
        if (!_pipeClient.IsConnected)
            throw new InvalidOperationException("Named pipe is not connected.");

        Packet? incomingPacket = await _packetHandler.ReadPacketAsync(_pipeClient);

        if (incomingPacket == null)
            throw new InvalidOperationException("Response not recognized.");
        
        return _packetDeserializer.Deserialize<Response>(incomingPacket);
    }

    public async Task<string> ReadPipeStdInAsync()
    {
        return await _pipeStandardIn.ReadLineAsync() ?? string.Empty;
    }
}
