namespace Tiempito.IPC.NET.Messages;

/// <summary>
/// Represents a request from a client.
/// </summary>
/// <param name="CommandType">A <see cref="CommandType"/> representing the group of commands.</param>
/// <param name="SubcommandType">Main command's subcommand type.</param>
/// <param name="Arguments">A <see cref="KeyValuePair{TKey,TValue}"/></param>
public record Request(string CommandType, string SubcommandType, IReadOnlyDictionary<string, string> Arguments);
