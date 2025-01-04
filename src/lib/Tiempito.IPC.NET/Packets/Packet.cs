namespace Tiempito.IPC.NET.Packets;

/// <summary>
/// Represents a message sent by a server or client.
/// </summary>
/// <param name="Length">Length of the message.</param>
/// <param name="Data">Actual data of the message.</param>
public record Packet(int Length, string Data);
