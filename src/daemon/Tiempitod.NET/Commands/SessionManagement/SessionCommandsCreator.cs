using Tiempitod.NET.Exceptions;
using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands.SessionManagement;

/// <summary>
/// Creates commands to manage sessions.
/// </summary>
public class SessionCommandsCreator : CommandCreator
{
    private readonly ISessionManager _sessionManager;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SessionCommandsCreator"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="sessionManager">Service to manage sessions.</param>
    public SessionCommandsCreator(
        ILogger<CommandCreator> logger,
        ISessionManager sessionManager)
        : base(logger, CommandType.Session)
    {
        _sessionManager = sessionManager;
    }

    /// <inheritdoc/>
    public override ICommand Create(string subcommandType, IReadOnlyDictionary<string, string> args)
    {
        switch (subcommandType)
        {
            case "start":
                return new StartSessionCommand(_sessionManager, args);
            case "pause":
                return new PauseSessionCommand(_sessionManager, args);
            case "resume":
                return new ResumeSessionCommand(_sessionManager, args);
            case "cancel":
                return new CancelSessionCommand(_sessionManager, args);
            default:
                _logger.LogError("Unrecognized subcommand was sent to the daemon: {SubcommandType}", subcommandType);
                throw new CommandNotFoundException(subcommandType);
        }
    }
}