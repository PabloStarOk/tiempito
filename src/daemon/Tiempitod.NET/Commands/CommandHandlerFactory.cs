using Tiempitod.NET.Exceptions;
using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands;

/// <summary>
/// A factory to create command handlers according to its nature.
/// </summary>
public class CommandHandlerFactory : ICommandHandlerFactory
{
    private readonly ISessionManager _sessionManager;
    
    /// <summary>
    /// Instantiates a new <see cref="CommandHandlerFactory"/>
    /// </summary>
    /// <param name="sessionManager">Manager of sessions.</param>
    public CommandHandlerFactory(ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }
    
    /// <summary>
    /// Creates an <see cref="ICommandHandler"/>.
    /// </summary>
    /// <param name="commandType">Command type the <see cref="ICommandHandler"/> must handle.</param>
    /// <returns>An <see cref="ICommandHandler"/> that can handle the given command type.</returns>
    /// <exception cref="CommandNotFoundException">If the given command is not recognized.</exception>
    public ICommandHandler CreateHandler(string commandType)
    {
        return commandType switch
        {
            "session" => new SessionCommandHandler(_sessionManager),
            _ => throw new CommandNotFoundException(commandType)
        };
    }
}
