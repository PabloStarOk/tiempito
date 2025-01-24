using Tiempitod.NET.Configuration.Session;
using Tiempitod.NET.Configuration.User;
using Tiempitod.NET.Exceptions;
using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands;

// TODO: Should this class be injected with the handlers and provide them with private variables?
/// <summary>
/// A factory to create command handlers according to its nature.
/// </summary>
public class CommandHandlerFactory : ICommandHandlerFactory
{
    private readonly IUserConfigProvider _userConfigProvider;
    private readonly ISessionConfigProvider _sessionConfigProvider;
    private readonly ISessionManager _sessionManager;
    
    /// <summary>
    /// Instantiates a new <see cref="CommandHandlerFactory"/>
    /// </summary>
    /// <param name="userConfigProvider">Provider of user's configuration.</param>
    /// <param name="sessionConfigProvider">Provider of session configurations.</param>
    /// <param name="sessionManager">Manager of sessions.</param>
    public CommandHandlerFactory(
        IUserConfigProvider userConfigProvider, 
        ISessionConfigProvider sessionConfigProvider, 
        ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;
        _userConfigProvider = userConfigProvider;
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
            "session" => new SessionCommandsHandler(_sessionConfigProvider, _sessionManager),
            "config" => new ConfigCommandsHandler(_userConfigProvider),
            _ => throw new CommandNotFoundException(commandType)
        };
    }
}
