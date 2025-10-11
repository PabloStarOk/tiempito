using MessagePack;

namespace Tiempito.IPC.Models;

/// <summary>
/// Represents a request from a client.
/// </summary>
/// <param name="CommandType">A <see cref="CommandType"/> representing the group of commands.</param>
/// <param name="SubcommandType">Main command's subcommand type.</param>
/// <param name="Arguments">A <see cref="KeyValuePair{TKey,TValue}"/>.</param>
/// <param name="RedirectProgress">If the client will keep connected to send progress messages of a session.</param>
[MessagePackObject]
public record Request(
    [property: Key(0)] string CommandType,
    [property: Key(1)] string SubcommandType,
    [property: Key(2)] IReadOnlyDictionary<string, string> Arguments,
    [property: Key(3)] bool RedirectProgress = false);
