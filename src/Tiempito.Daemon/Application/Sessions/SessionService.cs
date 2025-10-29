using Microsoft.Extensions.Options;

using Tiempito.Daemon.Application.Config.Sessions;
using Tiempito.Daemon.Application.Notifications;
using Tiempito.Daemon.Application.Shared.Abstractions;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Notifications.Enums;
using Tiempito.Daemon.Domain.Sessions;
using Tiempito.Daemon.Domain.Sessions.Enums;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.Daemon.Server;
using Tiempito.Daemon.Server.Configuration;

namespace Tiempito.Daemon.Application.Sessions;


/// <summary>
/// Service to manage sessions.
/// </summary>
public sealed class SessionService : Service, ISessionService
{
    private readonly ISessionConfigService _sessionConfigService;
    private readonly NotificationConfig _notificationConfig;
    private readonly INotificationService _notificationService;
    private readonly IStandardOutQueue _standardOutQueue;
    private readonly TimeProvider _timeProvider;
    private readonly Dictionary<string, Session> _activeSessions;

    public SessionService(
        ILogger<SessionService> logger,
        ISessionConfigService sessionConfigService,
        IOptions<NotificationConfig> notificationOptions,
        INotificationService notificationService,
        IStandardOutQueue standardOutQueue,
        TimeProvider timeProvider,
        Dictionary<string, Session>? activeSessions = null)
        : base(logger)
    {
        _sessionConfigService = sessionConfigService;
        _notificationConfig = notificationOptions.Value;
        _notificationService = notificationService;
        _standardOutQueue = standardOutQueue;
        _timeProvider = timeProvider;
        _activeSessions = activeSessions ?? new Dictionary<string, Session>();
    }

    protected override Task<bool> OnStartServiceAsync()
    {
        return Task.FromResult(true);
    }
    
    protected override Task<bool> OnStopServiceAsync()
    {
        return Task.FromResult(true);
    }
    
    public OperationResult StartSession(string sessionId = "", string sessionConfigId = "")
    {
        // Try to get the config
        SessionConfig sessionConfig;
        if (string.IsNullOrWhiteSpace(sessionConfigId))
            sessionConfig = _sessionConfigService.DefaultConfig;
        else if (_sessionConfigService.TryGetConfigById(sessionConfigId, out SessionConfig foundSessionConfig))
            sessionConfig = foundSessionConfig;
        else
            return new OperationResult(Success: false, Message: $"Session configuration with ID '{sessionConfigId}' was not found");

        // Use session config ID in empty string case
        if (string.IsNullOrWhiteSpace(sessionId))
            sessionId = sessionConfig.Id;
        
        // Verify if the session id already exists.
        if (_activeSessions.ContainsKey(sessionId))
            return new OperationResult(Success: false, Message: "There's already a started session with the same ID.");

        var session = Session.Create(
            sessionId,
            sessionConfig,
            _timeProvider,
            OnSessionSecondElapsed,
            OnSessionIntervalCompleted,
            OnSessionCompleted);

        _activeSessions.Add(session.Id, session);
        session.Start();
        OnSessionStarted(session);
        
        return new OperationResult(Success: true, Message: "Session started.");
    }

    public OperationResult PauseSession(string sessionId = "")
    {
        var executingSessions = _activeSessions
            .Where(s => s.Value.State.Status is SessionStatus.Executing)
            .ToDictionary(s => s.Key, s => s.Value);
        
        if (executingSessions.Count < 1 )
            return new OperationResult(Success: false, Message: "There are no running sessions to pause.");

        Session? session = null;
        if (!string.IsNullOrWhiteSpace(sessionId) && !executingSessions.TryGetValue(sessionId, out session))
            return new OperationResult(Success: false, Message: $"Running session with ID '{sessionId}' was not found.");

        if (string.IsNullOrWhiteSpace(sessionId) || session is null)
        {
            session = executingSessions.First().Value;
        }

        session.Pause();

        return new OperationResult(Success: true, Message: "Session paused.");
    }

    public OperationResult ResumeSession(string sessionId = "")
    {
        var pausedSessions = _activeSessions
            .Where(s => s.Value.State.Status is SessionStatus.Paused)
            .ToDictionary(s => s.Key, s => s.Value);

        if (pausedSessions.Count < 1 )
            return new OperationResult(Success: false, Message: "There are no paused sessions to resume.");

        Session? session = null;
        if (!string.IsNullOrWhiteSpace(sessionId) && !pausedSessions.TryGetValue(sessionId, out session))
            return new OperationResult(Success: false, Message: $"Paused session with ID '{sessionId}' was not found.");

        if (string.IsNullOrWhiteSpace(sessionId) || session is null)
        {
            session = pausedSessions.First().Value;
        }

        session.Resume();
        return new OperationResult(Success: true, Message: "Session resumed.");
    }

    public OperationResult CancelSession(string sessionId = "")
    {
        if (_activeSessions.Count < 1)
            return new OperationResult(Success: false, Message: "There are no sessions to cancel.");
        
        Session? session = null;
        if (!string.IsNullOrWhiteSpace(sessionId) && !_activeSessions.Remove(sessionId, out session))
            return new OperationResult(Success: false, Message: $"Started session with ID '{sessionId}' was not found.");
        
        if (string.IsNullOrWhiteSpace(sessionId) || session is null)
        {
            session = _activeSessions.First().Value;
            _activeSessions.Remove(session.Id);
        }

        session.CancelAsync().GetAwaiter().GetResult();
        return new OperationResult(Success: true, Message: "Session cancelled.");
    }

    private void OnSessionStarted(Session _)
    {
        _notificationService.CloseLastNotificationAsync().GetAwaiter().GetResult();
        _notificationService.NotifyAsync(
            summary: _notificationConfig.SessionStartedSummary,
            body: _notificationConfig.SessionStartedBody,
            NotificationSoundType.SessionStarted).GetAwaiter().GetResult();
    }

    private void OnSessionSecondElapsed(Session session)
    {
        var message = $"{session.State.IntervalType.ToString()} time: {session.State.ElapsedTime}";
        _standardOutQueue.QueueMessage(message);
    }

    private void OnSessionIntervalCompleted(Session session)
    {
        if (session.State.IntervalType is SessionIntervalType.Delay)
        {
            return;
        }

        var message = $"{session.State.IntervalType.ToString()} time completed.";
        _standardOutQueue.QueueMessage(message);

        _notificationService.CloseLastNotificationAsync().GetAwaiter().GetResult();

        string summary;
        string body;

        if (session.State.IntervalType is SessionIntervalType.Focus)
        {
            summary = _notificationConfig.FocusCompletedSummary;
            body = _notificationConfig.FocusCompletedBody;
        }
        else
        {
            summary = _notificationConfig.BreakCompletedSummary;
            body = _notificationConfig.BreakCompletedBody;
        }

        _notificationService.NotifyAsync(summary, body, NotificationSoundType.TimeCompleted).GetAwaiter().GetResult();
    }

    private void OnSessionCompleted(Session session)
    {
        _activeSessions.Remove(session.Id);

        var message = $"Session with id {session.Id} was completed";
        _standardOutQueue.QueueMessage(message);

        _notificationService.CloseLastNotificationAsync();
        _notificationService.NotifyAsync(
            summary: _notificationConfig.SessionFinishedSummary,
            body: _notificationConfig.SessionFinishedBody,
            NotificationSoundType.SessionFinished);
    }
}
