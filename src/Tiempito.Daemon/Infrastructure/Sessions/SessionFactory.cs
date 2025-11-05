using Tiempito.Daemon.Application.Config.Sessions;
using Tiempito.Daemon.Application.Sessions;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Sessions;

namespace Tiempito.Daemon.Infrastructure.Sessions;

/// <summary>
/// Factory for creating <see cref="Session"/> instances with specified configuration and event handlers.
/// </summary>
internal sealed class SessionFactory : ISessionFactory
{
    private readonly ILogger<SessionFactory> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ISessionConfigService _sessionConfigService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionFactory"/> class.
    /// </summary>
    /// <param name="logger">Logger to register events.</param>
    /// <param name="loggerFactory">Factory for creating loggers.</param>
    /// <param name="timeProvider">Provider for time-related operations.</param>
    /// <param name="sessionConfigService">Service for session configuration management.</param>
    public SessionFactory(
        ILogger<SessionFactory> logger,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider,
        ISessionConfigService sessionConfigService)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _timeProvider = timeProvider;
        _sessionConfigService = sessionConfigService;
    }

    /// <inheritdoc/>
    public ISession Create(string id, string configId)
    {
        if (!string.IsNullOrWhiteSpace(configId) && !ExistsConfig(configId))
        {
            throw new ArgumentException($"Session configuration with ID '{configId}' not found");
        }

        SessionConfig? sessionConfig;
        if (string.IsNullOrWhiteSpace(configId))
        {
            sessionConfig = _sessionConfigService.DefaultConfig;
        }
        else
        {
            _sessionConfigService.TryGetConfigById(configId, out sessionConfig);
        }

        var logger = _loggerFactory.CreateLogger<Session>();
        var periodicTimer = new PeriodicTimer(Session.SecondInterval, _timeProvider);
        var timerWrapper = new DefaultPeriodicTimer(periodicTimer);
        var sessionId = string.IsNullOrWhiteSpace(id) ? sessionConfig!.Id : id;
        var session = Session.Create(logger, sessionId, sessionConfig!, timerWrapper);
        _logger.LogInformation("Session with ID '{Id}' created using config '{ConfigId}'", sessionId, session.Configuration.Id);
        return session;
    }

    /// <inheritdoc/>
    public bool ExistsConfig(string configId)
    {
        return _sessionConfigService.TryGetConfigById(configId, out _);
    }
}