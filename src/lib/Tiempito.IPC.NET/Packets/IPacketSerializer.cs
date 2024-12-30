namespace Tiempito.IPC.NET.Packets;

/// <summary>
/// Defines a JSON serializer of objects to store in the data of a packet.
/// </summary>
public interface IPacketSerializer
{
    /// <summary>
    /// Serializes an object into the data of a packet.
    /// </summary>
    /// <param name="obj">Object to serialize.</param>
    public Packet Serialize(object obj);
}
