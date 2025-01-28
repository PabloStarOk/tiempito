using Tiempitod.NET.Configuration.Session;
using Tiempitod.NET.Configuration.User;
using Tiempitod.NET.Exceptions;

namespace Tiempitod.NET.Commands.Configuration;

/// <summary>
/// Creates commands to manage user and sessions configurations.
/// </summary>
public class ConfigCommandsCreator : CommandCreator
{
    private readonly IUserConfigProvider _userConfigProvider;
    private readonly ISessionConfigProvider _sessionConfigProvider;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigCommandsCreator"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="userConfigProvider">Service to manage user configurations.</param>
    /// <param name="sessionConfigProvider">Service to manage session configurations.</param>
    public ConfigCommandsCreator(
        ILogger<CommandCreator> logger,
        IUserConfigProvider userConfigProvider,
        ISessionConfigProvider sessionConfigProvider)
        : base(logger, CommandType.Config)
    {
        _userConfigProvider = userConfigProvider;
        _sessionConfigProvider = sessionConfigProvider;
    }

    /// <inheritdoc/>
    public override ICommand Create(string subcommandType, IReadOnlyDictionary<string, string> args)
    {
        switch (subcommandType)
        {
            case "set":
                return new SetConfigCommand(_userConfigProvider, args);
            case "enable":
                return new EnableConfigCommand(_userConfigProvider, args);
            case "disable":
                return new DisableConfigCommand(_userConfigProvider, args);
            case "create":
                return new CreateSessionConfigCommand(_sessionConfigProvider, args);
            case "modify":
                return new ModifySessionConfigCommand(_sessionConfigProvider, args);
            default:
                _logger.LogError("Unrecognized subcommand was sent to the daemon: {SubcommandType}", subcommandType);
                throw new CommandNotFoundException(subcommandType);
        }
    }   
}