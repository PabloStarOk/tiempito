namespace Tiempito.Daemon.Common.Exceptions;

/// <summary>
/// Exception that is thrown when the given command is not recognized.
/// </summary>
/// <param name="command">Unrecognized command.</param>
public class CommandNotFoundException(string command) : Exception($"Command {command} is not recognized.");
