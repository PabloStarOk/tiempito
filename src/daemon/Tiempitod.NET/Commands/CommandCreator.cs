using Tiempitod.NET.Common.Exceptions;

namespace Tiempitod.NET.Commands;

/// <summary>
/// Defines the base for creators of command.
/// </summary>
public abstract class CommandCreator
{
    protected readonly ILogger<CommandCreator> _logger;
    public CommandType CommandType { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandCreator"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging operations.</param>
    /// <param name="commandType">The type of command to be created.</param>
    public CommandCreator(ILogger<CommandCreator> logger, CommandType commandType)
    {
        _logger = logger;
        CommandType = commandType;
    }
    
    /// <summary>
    /// Creates a command that matches the subcommand type.
    /// </summary>
    /// <param name="subcommandType">Subcommand type to create the command.</param>
    /// <param name="args">Arguments to create the command.</param>
    /// <returns>A <see cref="ICommand"/>.</returns>
    /// <exception cref="CommandNotFoundException">If the subcommand type is not recognized.</exception>
    public abstract ICommand Create(string subcommandType, IReadOnlyDictionary<string, string> args);
}