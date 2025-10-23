using MessagePack;

namespace Tiempito.IPC.Models;

/// <summary>
/// Represents a command.
/// </summary>
[MessagePackObject]
public record Command : Message
{
    /// <summary>
    /// Gets the command group or type (the main category of the command).
    /// </summary>
    [Key(3)]
    public string CommandType { get; }

    /// <summary>
    /// Gets the subcommand type within the main command.
    /// </summary>
    [Key(4)]
    public string SubcommandType { get; }

    /// <summary>
    /// Gets a read-only dictionary of argument name/value pairs for the command.
    /// </summary>
    [Key(5)]
    public IReadOnlyDictionary<string, string> Arguments { get; }

    /// <summary>
    /// Gets a value indicating whether the client will remain connected to receive progress messages.
    /// </summary>
    [Key(6)]
    public bool RedirectProgress { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Command"/> class.
    /// </summary>
    /// <param name="id">Unique identifier for the command.</param>
    /// <param name="correlationId">Identifier used to correlate this command with other messages.</param>
    /// <param name="timestamp">Timestamp when the command was created.</param>
    /// <param name="commandType">The command group or type.</param>
    /// <param name="subcommandType">The subcommand type within the main command.</param>
    /// <param name="arguments">Read-only dictionary of argument name/value pairs.</param>
    /// <param name="redirectProgress">If true, client stays connected to receive progress messages; defaults to false.</param>
    public Command(
        Guid id,
        Guid correlationId,
        DateTimeOffset timestamp,
        string commandType,
        string subcommandType,
        IReadOnlyDictionary<string, string> arguments,
        bool redirectProgress = false)
        : base(id, correlationId, timestamp)
    {
        CommandType = commandType;
        SubcommandType = subcommandType;
        Arguments = arguments;
        RedirectProgress = redirectProgress;
    }

    /// <summary>
    /// Creates a new <see cref="Command"/> with a generated id and correlation id and the current UTC timestamp.
    /// </summary>
    /// <param name="commandType">The command group or main command type.</param>
    /// <param name="subcommandType">The subcommand type within the main command.</param>
    /// <param name="arguments">Read-only dictionary of argument name/value pairs for the command.</param>
    /// <param name="redirectProgress">If true, the client will remain connected to receive progress messages; defaults to false.</param>
    /// <returns>A newly created <see cref="Command"/> instance.</returns>
    public static Command CreateNew(
        string commandType,
        string subcommandType,
        IReadOnlyDictionary<string, string> arguments,
        bool redirectProgress = false)
    {
        return new Command(
            id: Guid.NewGuid(),
            correlationId: Guid.NewGuid(),
            timestamp: DateTimeOffset.UtcNow,
            commandType,
            subcommandType,
            arguments,
            redirectProgress);
    }
}
