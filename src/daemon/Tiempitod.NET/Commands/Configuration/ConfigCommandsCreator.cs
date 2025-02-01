using Tiempitod.NET.Configuration.Session;
using Tiempitod.NET.Configuration.User.Interfaces;
using Tiempitod.NET.Exceptions;

namespace Tiempitod.NET.Commands.Configuration;

/// <summary>
/// Creates commands to manage user and sessions configurations.
/// </summary>
public class ConfigCommandsCreator : CommandCreator
{
    private readonly IUserConfigService _userConfigService;
    private readonly ISessionConfigService _sessionConfigService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigCommandsCreator"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="userConfigService">Service of user's configurations.</param>
    /// <param name="sessionConfigService">Service to manage session configurations.</param>
    public ConfigCommandsCreator(
        ILogger<ConfigCommandsCreator> logger,
        IUserConfigService userConfigService,
        ISessionConfigService sessionConfigService)
        : base(logger, CommandType.Config)
    {
        _userConfigService = userConfigService;
        _sessionConfigService = sessionConfigService;
    }

    /// <inheritdoc/>
    public override ICommand Create(string subcommandType, IReadOnlyDictionary<string, string> args)
    {
        switch (subcommandType)
        {
            case "set":
                return new SetConfigCommand(_userConfigService, args);
            case "enable":
                return new EnableConfigCommand(_userConfigService, args);
            case "disable":
                return new DisableConfigCommand(_userConfigService, args);
            case "create-session-config":
                return new CreateSessionConfigCommand(_sessionConfigService, args);
            case "modify-session-config":
                return new ModifySessionConfigCommand(_sessionConfigService, args);
            default:
                _logger.LogError("Unrecognized subcommand was sent to the daemon: {SubcommandType}", subcommandType);
                throw new CommandNotFoundException(subcommandType);
        }
    }   
}