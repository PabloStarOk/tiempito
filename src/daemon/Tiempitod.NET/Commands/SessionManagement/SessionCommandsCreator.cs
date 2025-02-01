using Tiempitod.NET.Common.Exceptions;
using Tiempitod.NET.Sessions.Interfaces;

namespace Tiempitod.NET.Commands.SessionManagement;

/// <summary>
/// Creates commands to manage sessions.
/// </summary>
public class SessionCommandsCreator : CommandCreator
{
    private readonly ISessionService _sessionService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SessionCommandsCreator"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="sessionService">Service to manage sessions.</param>
    public SessionCommandsCreator(
        ILogger<CommandCreator> logger,
        ISessionService sessionService)
        : base(logger, CommandType.Session)
    {
        _sessionService = sessionService;
    }

    /// <inheritdoc/>
    public override ICommand Create(string subcommandType, IReadOnlyDictionary<string, string> args)
    {
        switch (subcommandType)
        {
            case "start":
                return new StartSessionCommand(_sessionService, args);
            case "pause":
                return new PauseSessionCommand(_sessionService, args);
            case "resume":
                return new ResumeSessionCommand(_sessionService, args);
            case "cancel":
                return new CancelSessionCommand(_sessionService, args);
            default:
                _logger.LogError("Unrecognized subcommand was sent to the daemon: {SubcommandType}", subcommandType);
                throw new CommandNotFoundException(subcommandType);
        }
    }
}