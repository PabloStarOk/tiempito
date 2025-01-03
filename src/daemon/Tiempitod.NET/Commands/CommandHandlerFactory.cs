using Tiempitod.NET.Configuration.Session;
using Tiempitod.NET.Exceptions;
using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands;

// TODO: Should this class be injected with the handlers and provide them with private variables?
/// <summary>
/// A factory to create command handlers according to its nature.
/// </summary>
public class CommandHandlerFactory : ICommandHandlerFactory
{
    private readonly ISessionConfigProvider _sessionConfigProvider;
    private readonly ISessionManager _sessionManager;
    
    /// <summary>
    /// Instantiates a new <see cref="CommandHandlerFactory"/>
    /// </summary>
    /// <param name="sessionConfigProvider">Provider of session configurations.</param>
    /// <param name="sessionManager">Manager of sessions.</param>
    public CommandHandlerFactory(
        ISessionConfigProvider sessionConfigProvider, 
        ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;
        _sessionConfigProvider = sessionConfigProvider;
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
            "session" => new SessionCommandHandler(_sessionConfigProvider, _sessionManager),
            _ => throw new CommandNotFoundException(commandType)
        };
    }
}
