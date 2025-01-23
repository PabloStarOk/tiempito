namespace Tiempito.IPC.NET.Messages;

/// <summary>
/// Represents a request from a client.
/// </summary>
/// <param name="CommandType">A <see cref="CommandType"/> representing the group of commands.</param>
/// <param name="SubcommandType">Main command's subcommand type.</param>
/// <param name="Arguments">A <see cref="KeyValuePair{TKey,TValue}"/>.</param>
/// <param name="RedirectProgress">If the client will keep connected to send progress messages of a session.</param>
public record Request(string CommandType, string SubcommandType, IReadOnlyDictionary<string, string> Arguments, bool RedirectProgress = false);
