namespace Tiempito.IPC.NET.Messages;

/// <summary>
/// Represents a request from a client.
/// </summary>
/// <param name="Command">Command of the client.</param>
public record Request(string Command); 
