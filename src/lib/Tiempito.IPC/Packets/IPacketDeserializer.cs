namespace Tiempito.IPC.Packets;

/// <summary>
/// Defines a deserializer of packets to cast data of packets into objects.
/// </summary>
public interface IPacketDeserializer
{
    /// <summary>
    /// Deserializes the data of a packet into an object.
    /// </summary>
    /// <param name="packet">Packet to deserialize.</param>
    /// <typeparam name="TResult">Type to deserialize to.</typeparam>
    /// <returns>An object of type <see cref="TResult"/>.</returns>
    public TResult Deserialize<TResult>(Packet packet);
}