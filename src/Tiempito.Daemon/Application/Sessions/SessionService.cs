using System.Collections.ObjectModel;

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
    private readonly ISessionStorage _sessionStorage;
    private readonly IStandardOutQueue _standardOutQueue;
    private readonly TimeProvider _timeProvider;
    private CancellationTokenSource _timerTokenSource;

    public SessionService(
        ILogger<SessionService> logger,
        ISessionConfigService sessionConfigService,
        IOptions<NotificationConfig> notificationOptions,
        INotificationService notificationService,
        ISessionStorage sessionStorage,
        IStandardOutQueue standardOutQueue,
        TimeProvider timeProvider) : base(logger)
    {
        _sessionConfigService = sessionConfigService;
        _notificationConfig = notificationOptions.Value;
        _notificationService = notificationService;
        _sessionStorage = sessionStorage;
        _timerTokenSource = new CancellationTokenSource();
        _standardOutQueue = standardOutQueue;
        _timeProvider = timeProvider;
    }

    protected override Task<bool> OnStartServiceAsync()
    {
        return Task.FromResult(true);
    }
    
    protected async override Task<bool> OnStopServiceAsync()
    {
        if (!_timerTokenSource.IsCancellationRequested)
            await _timerTokenSource.CancelAsync();
        _timerTokenSource.Dispose();

        return true;
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
        ReadOnlyDictionary<string, Session> startedSessions = 
            _sessionStorage.RunningSessions.Concat(_sessionStorage.PausedSessions).ToDictionary().AsReadOnly();
        if (startedSessions.ContainsKey(sessionId))
            return new OperationResult(Success: false, Message: "There's already a started session with the same ID.");

        var session = Session.Create(
            sessionId,
            sessionConfig,
            _timeProvider,
            OnSessionSecondElapsed,
            OnSessionIntervalCompleted,
            OnSessionCompleted);

        _timerTokenSource = RegenerateTokenSource(_timerTokenSource);
        _sessionStorage.AddSession(SessionStatus.Executing, session);
        session.Start();
        OnSessionStarted(session);
        
        return new OperationResult(Success: true, Message: "Session started.");
    }

    public OperationResult PauseSession(string sessionId = "")
    {
        if (_sessionStorage.RunningSessions.Count < 1 )
            return new OperationResult(Success: false, Message: "There are no running sessions to pause.");

        Session? session = null;
        if (!string.IsNullOrWhiteSpace(sessionId)
            && !_sessionStorage.RunningSessions.TryGetValue(sessionId, out session))
            return new OperationResult(Success: false, Message: $"Running session with ID '{sessionId}' was not found.");

        if (string.IsNullOrWhiteSpace(sessionId) || session is null)
        {
            session = _sessionStorage.RunningSessions.First().Value;
        }

        _sessionStorage.RemoveSession(SessionStatus.Executing, session.Id);
        _sessionStorage.AddSession(SessionStatus.Paused, session);
        session.Pause();

        return new OperationResult(Success: true, Message: "Session paused.");
    }

    public OperationResult ResumeSession(string sessionId = "")
    {
        if (_sessionStorage.PausedSessions.Count < 1 )
            return new OperationResult(Success: false, Message: "There are no paused sessions to resume.");

        Session? session = null;
        if (!string.IsNullOrWhiteSpace(sessionId) 
            && !_sessionStorage.PausedSessions.TryGetValue(sessionId, out session))
            return new OperationResult(Success: false, Message: $"Paused session with ID '{sessionId}' was not found.");

        if (string.IsNullOrWhiteSpace(sessionId) || session is null)
        {
            session = _sessionStorage.PausedSessions.First().Value;
        }

        _sessionStorage.RemoveSession(SessionStatus.Paused, session.Id);
        _sessionStorage.AddSession(SessionStatus.Executing, session);
        session.Resume();

        return new OperationResult(Success: true, Message: "Session resumed.");
    }

    public OperationResult CancelSession(string sessionId = "")
    {
        ReadOnlyDictionary<string, Session> startedSessions = 
            _sessionStorage.RunningSessions.Concat(_sessionStorage.PausedSessions).ToDictionary().AsReadOnly();
        
        if (startedSessions.Count < 1)
            return new OperationResult(Success: false, Message: "There are no sessions to cancel.");
        
        Session? session = null;
        if (!string.IsNullOrWhiteSpace(sessionId) 
            && !startedSessions.TryGetValue(sessionId, out session))
            return new OperationResult(Success: false, Message: $"Started session with ID '{sessionId}' was not found.");
        
        if (string.IsNullOrWhiteSpace(sessionId) || session is null)
        {
            session = startedSessions.First().Value;
        }

        _sessionStorage.RemoveSession(
                session.State.Status is SessionStatus.Executing ? SessionStatus.Executing : SessionStatus.Paused,
                session.Id);
        _sessionStorage.AddSession(SessionStatus.Cancelled, session);
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
        _sessionStorage.AddSession(SessionStatus.Finished, session);

        var message = $"Session with id {session.Id} was completed";
        _standardOutQueue.QueueMessage(message);

        _notificationService.CloseLastNotificationAsync();
        _notificationService.NotifyAsync(
            summary: _notificationConfig.SessionFinishedSummary,
            body: _notificationConfig.SessionFinishedBody,
            NotificationSoundType.SessionFinished);
    }
}
