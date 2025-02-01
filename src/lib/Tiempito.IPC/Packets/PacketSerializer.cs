using System.Text.Json;

using Tiempito.IPC.Packets.Interfaces;
using Tiempito.IPC.Packets.Objects;

namespace Tiempito.IPC.Packets;

/// <summary>
/// JSON serializer of objects.
/// </summary>
public class PacketSerializer : IPacketSerializer
{
    private readonly JsonSerializerOptions _serializerOptions;
    
    public PacketSerializer(JsonSerializerOptions serializerOptions)
    {
        _serializerOptions = serializerOptions;
    }
    
    public Packet Serialize(object obj)
    {
        string serializedObject = JsonSerializer.Serialize(obj, _serializerOptions);
        return new Packet(serializedObject.Length, serializedObject);
    }
}
