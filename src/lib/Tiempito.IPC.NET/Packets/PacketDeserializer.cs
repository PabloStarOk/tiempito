using System.Text.Json;

namespace Tiempito.IPC.NET.Packets;

/// <summary>
/// Provides deserialization of packets.
/// </summary>
public class PacketDeserializer : IPacketDeserializer
{
    private readonly JsonSerializerOptions _serializerOptions;
    
    public PacketDeserializer(JsonSerializerOptions serializerOptions)
    {
        _serializerOptions = serializerOptions;
    }
    
    public TResult Deserialize<TResult>(Packet packet)
    {
        ArgumentNullException.ThrowIfNull(packet);
        
        object? responseObj = JsonSerializer.Deserialize(packet.Data, typeof(TResult), _serializerOptions);
        
        if (responseObj == null)
            throw new InvalidOperationException("Request not recognized.");
        
        return (TResult) responseObj;
    }
}
