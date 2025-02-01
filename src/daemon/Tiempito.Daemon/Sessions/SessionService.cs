using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;

using Tiempito.Daemon.Common;
using Tiempito.Daemon.Configuration.Daemon.Objects;
using Tiempito.Daemon.Configuration.Session.Interfaces;
using Tiempito.Daemon.Configuration.Session.Objects;
using Tiempito.Daemon.Notifications.Enums;
using Tiempito.Daemon.Notifications.Interfaces;
using Tiempito.Daemon.Server.Interfaces;
using Tiempito.Daemon.Sessions.Enums;
using Tiempito.Daemon.Sessions.Interfaces;
using Tiempito.Daemon.Sessions.Objects;

namespace Tiempito.Daemon.Sessions;

/// <summary>
/// Service to manage sessions.
/// </summary>
public sealed class SessionService : Service, ISessionService
{
    private readonly ISessionConfigService _sessionConfigService;
    private readonly NotificationConfig _notificationConfig;
    private readonly Progress<Session> _progress;
    private readonly INotificationService _notificationService;
    private readonly ISessionStorage _sessionStorage;
    private readonly ISessionTimer _sessionTimer;
    private readonly IStandardOutQueue _standardOutQueue;
    private CancellationTokenSource _timerTokenSource;
    
    public SessionService(
        ILogger<SessionService> logger,
        ISessionConfigService sessionConfigService,
        IOptions<NotificationConfig> notificationOptions,
        INotificationService notificationService,
        Progress<Session> progress,
        ISessionStorage sessionStorage,
        ISessionTimer sessionTimer,
        IStandardOutQueue standardOutQueue) : base(logger)
    {
        _sessionConfigService = sessionConfigService;
        _notificationConfig = notificationOptions.Value;
        _progress = progress;
        _notificationService = notificationService;
        _sessionTimer = sessionTimer;
        _sessionStorage = sessionStorage;
        _timerTokenSource = new CancellationTokenSource();
        _standardOutQueue = standardOutQueue;
    }

    protected override Task<bool> OnStartServiceAsync()
    {
        _progress.ProgressChanged += ProgressEventHandler;
        _sessionTimer.OnTimeCompleted += TimeCompletedHandler;
        _sessionTimer.OnDelayElapsed += DelayProgressHandler;
        _sessionTimer.OnSessionStarted += SessionStartedHandler;
        _sessionTimer.OnSessionCompleted += SessionCompletedHandler;
        return Task.FromResult(true);
    }
    
    protected async override Task<bool> OnStopServiceAsync()
    {
        if (!_timerTokenSource.IsCancellationRequested)
            await _timerTokenSource.CancelAsync();
        _timerTokenSource.Dispose();
        
        _progress.ProgressChanged -= ProgressEventHandler;
        _sessionTimer.OnTimeCompleted -= TimeCompletedHandler;
        _sessionTimer.OnDelayElapsed -= DelayProgressHandler;
        _sessionTimer.OnSessionStarted -= SessionStartedHandler;
        _sessionTimer.OnSessionCompleted -= SessionCompletedHandler;
        _sessionTimer.StopAll();
        return true;
    }
    
    public OperationResult StartSession(string sessionId = "", string sessionConfigId = "")
    {
        // Try to get the config
        SessionConfig configSession;
        if (string.IsNullOrWhiteSpace(sessionConfigId))
            configSession = _sessionConfigService.DefaultConfig;
        else if (_sessionConfigService.TryGetConfigById(sessionConfigId, out SessionConfig foundSessionConfig))
            configSession = foundSessionConfig;
        else
            return new OperationResult(Success: false, Message: $"Session configuration with ID '{sessionConfigId}' was not found");

        // Use session config ID in empty string case
        if (string.IsNullOrWhiteSpace(sessionId))
            sessionId = configSession.Id;
        
        // Verify if the session id already exists.
        ReadOnlyDictionary<string, Session> startedSessions = 
            _sessionStorage.RunningSessions.Concat(_sessionStorage.PausedSessions).ToDictionary().AsReadOnly();
        if (startedSessions.ContainsKey(sessionId))
            return new OperationResult(Success: false, Message: "There's already a started session with the same ID.");
        
        var sessionToStart = new Session(
            sessionId, configSession.TargetCycles, configSession.DelayBetweenTimes,
            configSession.FocusDuration, configSession.BreakDuration);
        
        _timerTokenSource = RegenerateTokenSource(_timerTokenSource);
        _sessionTimer.Start(sessionToStart, _timerTokenSource.Token);
        
        return new OperationResult(Success: true, Message: "Session started.");
    }

    public OperationResult PauseSession(string sessionId = "")
    {
        if (_sessionStorage.RunningSessions.Count < 1 )
            return new OperationResult(Success: false, Message: "There are no running sessions to pause.");
        
        if (!string.IsNullOrWhiteSpace(sessionId) 
            && !_sessionStorage.RunningSessions.ContainsKey(sessionId))
            return new OperationResult(Success: false, Message: $"Running session with ID '{sessionId}' was not found.");
        
        if (string.IsNullOrWhiteSpace(sessionId))
            sessionId = _sessionStorage.RunningSessions.First().Value.Id;
        
        Session pausedSession = _sessionTimer.Stop(sessionId);
        _sessionStorage.AddSession(SessionStatus.Paused, pausedSession);
        
        return new OperationResult(Success: true, Message: "Session paused.");
    }

    public OperationResult ResumeSession(string sessionId = "")
    {
        if (_sessionStorage.PausedSessions.Count < 1 )
            return new OperationResult(Success: false, Message: "There are no paused sessions to resume.");
        
        if (!string.IsNullOrWhiteSpace(sessionId) 
            && !_sessionStorage.PausedSessions.ContainsKey(sessionId))
            return new OperationResult(Success: false, Message: $"Paused session with ID '{sessionId}' was not found.");
        
        if (string.IsNullOrWhiteSpace(sessionId))
            sessionId = _sessionStorage.PausedSessions.First().Value.Id;
        
        Session resumedSession = _sessionStorage.RemoveSession(SessionStatus.Paused, sessionId);
        _sessionTimer.Start(resumedSession, _timerTokenSource.Token);
        
        return new OperationResult(Success: true, Message: "Session resumed.");
    }

    public OperationResult CancelSession(string sessionId = "")
    {
        ReadOnlyDictionary<string, Session> startedSessions = 
            _sessionStorage.RunningSessions.Concat(_sessionStorage.PausedSessions).ToDictionary().AsReadOnly();
        
        if (startedSessions.Count < 1)
            return new OperationResult(Success: false, Message: "There are no sessions to cancel.");
        
        if (!string.IsNullOrWhiteSpace(sessionId) 
            && !startedSessions.ContainsKey(sessionId))
            return new OperationResult(Success: false, Message: $"Started session with ID '{sessionId}' was not found.");
        
        if (string.IsNullOrWhiteSpace(sessionId))
            sessionId = startedSessions.First().Value.Id;

        Session cancelledSession = startedSessions[sessionId].Status is SessionStatus.Executing 
                ? _sessionTimer.Stop(sessionId)
                : _sessionStorage.RemoveSession(SessionStatus.Paused, sessionId);
        _sessionStorage.AddSession(SessionStatus.Cancelled, cancelledSession);
        
        return new OperationResult(Success: true, Message: "Session cancelled.");
    }
    
    /// <summary>
    /// Notifies to the user of the elapsed time in the stdout.
    /// </summary>
    /// <param name="sender">Sender of the report.</param>
    /// <param name="session">Session subject of the report.</param>
    private void ProgressEventHandler(object? sender, Session session)
    {
        var message = $"{session.CurrentTimeType.ToString()} time: {session.Elapsed}";
        _standardOutQueue.QueueMessage(message);
    }
    
    /// <summary>
    /// Notifies to the user in the stdout and with system notifications when
    /// a time is completed.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="timeType">A <see cref="TimeType"/> which represents the time completed.</param>
    private async Task TimeCompletedHandler(object? sender, TimeType timeType)
    {
        var message = $"{timeType.ToString()} time completed.";
        _standardOutQueue.QueueMessage(message);
        
        await _notificationService.CloseLastNotificationAsync();
        
        string summary;
        string body;

        if (timeType is TimeType.Focus)
        {
            summary = _notificationConfig.FocusCompletedSummary;
            body = _notificationConfig.FocusCompletedBody;
        }
        else
        {
            summary = _notificationConfig.BreakCompletedSummary;
            body = _notificationConfig.BreakCompletedBody;
        }
        
        await _notificationService.NotifyAsync(summary, body, NotificationSoundType.TimeCompleted);
    }

    /// <summary>
    /// Notifies to the user of a second elapsed in the delay
    /// between times.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="time">Elapsed time.</param>
    private Task DelayProgressHandler(object? sender, TimeSpan time)
    {
        var message = $"Elapsed delay time: {time}";
        _standardOutQueue.QueueMessage(message);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Notifies to the user with the stdout and system notifications when
    /// a session is started.
    /// </summary>
    /// <param name="e">Empty arguments of the invoked event.</param>
    /// <param name="sender">Sender of the event.</param>
    private async Task SessionStartedHandler(object? sender, EventArgs e)
    {
        await _notificationService.CloseLastNotificationAsync();
        await _notificationService.NotifyAsync(
            summary: _notificationConfig.SessionStartedSummary,
            body: _notificationConfig.SessionStartedBody,
            NotificationSoundType.SessionStarted);
    }
    
    /// <summary>
    /// Notifies to the user in the stdout and with system notifications when
    /// a session is completed.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="session">The completed <see cref="Session"/>.</param>
    private async Task SessionCompletedHandler(object? sender, Session session)
    {
        _sessionStorage.AddSession(SessionStatus.Finished, session);
        
        var message = $"Session with id {session.Id} was completed";
        _standardOutQueue.QueueMessage(message);
        
        await _notificationService.CloseLastNotificationAsync();
        await _notificationService.NotifyAsync(
            summary: _notificationConfig.SessionFinishedSummary,
            body: _notificationConfig.SessionFinishedBody,
            NotificationSoundType.SessionFinished);
    }
}
