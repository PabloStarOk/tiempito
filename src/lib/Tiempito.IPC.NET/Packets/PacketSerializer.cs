using System.Text.Json;

namespace Tiempito.IPC.NET.Packets;

/// <summary>
/// JSON serializer of objects.
/// </summary>
public class PacketSerializer : IPacketSerializer
{
    public Packet Serialize(object obj)
    {
        string serializedObject = JsonSerializer.Serialize(obj);
        return new Packet(serializedObject.Length, serializedObject);
    }
}
