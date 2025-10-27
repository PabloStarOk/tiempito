using Tiempito.Daemon.Application.Commands.Exceptions;
using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Config.Sessions;
using Tiempito.Daemon.Application.Config.User;
using Tiempito.Daemon.Domain.Commands.Enums;

namespace Tiempito.Daemon.Application.Commands.Config;

/// <summary>
/// Creates commands to manage user and sessions configurations.
/// </summary>
public class ConfigCommandsCreator : CommandCreator
{
    private readonly IUserConfigService _userConfigService;
    private readonly ISessionConfigService _sessionConfigService;
    private readonly ITimeSpanConverter _timeSpanConverter;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigCommandsCreator"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="userConfigService">Service of user's configurations.</param>
    /// <param name="sessionConfigService">Service to manage session configurations.</param>
    /// <param name="timeSpanConverter">Converter of time span.</param>
    public ConfigCommandsCreator(
        ILogger<ConfigCommandsCreator> logger,
        IUserConfigService userConfigService,
        ISessionConfigService sessionConfigService,
        ITimeSpanConverter timeSpanConverter)
        : base(logger, CommandType.Config)
    {
        _userConfigService = userConfigService;
        _sessionConfigService = sessionConfigService;
        _timeSpanConverter = timeSpanConverter;
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
                return new CreateSessionConfigCommand(_sessionConfigService, _timeSpanConverter, args);
            case "modify-session-config":
                return new ModifySessionConfigCommand(_sessionConfigService, _timeSpanConverter, args);
            default:
                _logger.LogError("Unrecognized subcommand was sent to the daemon: {SubcommandType}", subcommandType);
                throw new CommandNotFoundException(subcommandType);
        }
    }   
}