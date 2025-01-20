using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using Tiempitod.NET.Configuration.Notifications;
using Tiempitod.NET.Configuration.Session;
using Tiempitod.NET.Extensions;
using Tiempitod.NET.Notifications;

namespace Tiempitod.NET.Session;

/// <summary>
/// A concrete class to manage sessions.
/// </summary>
public sealed class SessionManager : DaemonService, ISessionManager
{
    private readonly ISessionConfigProvider _sessionConfigProvider;
    private readonly NotificationConfig _notificationConfig;
    private readonly Progress<Session> _progress;
    private readonly INotificationManager _notificationManager;
    private readonly ISessionStorage _sessionStorage;
    private readonly ISessionTimer _sessionTimer;
    private CancellationTokenSource _timerTokenSource;
    
    public SessionManager(
        ILogger<SessionManager> logger,
        ISessionConfigProvider sessionConfigProvider,
        IOptions<NotificationConfig> notificationOptions,
        INotificationManager notificationManager,
        Progress<Session> progress,
        ISessionStorage sessionStorage,
        ISessionTimer sessionTimer) : base(logger)
    {
        _sessionConfigProvider = sessionConfigProvider;
        _notificationConfig = notificationOptions.Value;
        _progress = progress;
        _notificationManager = notificationManager;
        _sessionTimer = sessionTimer;
        _sessionStorage = sessionStorage;
        _timerTokenSource = new CancellationTokenSource();
    }

    protected override void OnStartService()
    {
        _progress.ProgressChanged += ProgressEventHandler;
        _sessionTimer.OnTimeCompleted += TimeCompletedHandler;
        _sessionTimer.OnDelayElapsed += DelayProgressHandler;
        _sessionTimer.OnSessionStarted += SessionStartedHandler;
        _sessionTimer.OnSessionCompleted += SessionCompletedHandler;
    }
    
    protected override void OnStopService()
    {
        if (!_timerTokenSource.IsCancellationRequested)
            _timerTokenSource.Cancel();
        _timerTokenSource.Dispose();
        
        _progress.ProgressChanged -= ProgressEventHandler;
        _sessionTimer.OnTimeCompleted -= TimeCompletedHandler;
        _sessionTimer.OnDelayElapsed -= DelayProgressHandler;
        _sessionTimer.OnSessionStarted -= SessionStartedHandler;
        _sessionTimer.OnSessionCompleted -= SessionCompletedHandler;
        _sessionTimer.StopAll();
    }
    
    public OperationResult StartSession(string sessionId = "", string sessionConfigId = "")
    {
        // Try to get the config
        SessionConfig configSession;
        if (string.IsNullOrWhiteSpace(sessionConfigId))
            configSession = _sessionConfigProvider.DefaultSessionConfig;
        else if (_sessionConfigProvider.SessionConfigs.TryGetValue(sessionConfigId.ToLower(), out SessionConfig foundSessionConfig)) // TODO: Unify letter case management for sessionId.
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
        
        Logger.LogWarning("Session paused at time {Time}", DateTimeOffset.Now); // TODO: Replace with stdout.
        
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
        
        Logger.LogWarning("Continuing session at time {Time}", DateTimeOffset.Now); // TODO: Replace with stdout.
        
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
        
        Logger.LogWarning("Session cancelled at {Time}", DateTimeOffset.Now); // TODO: Replace with stdout.
        return new OperationResult(Success: true, Message: "Session cancelled.");
    }
    
    /// <summary>
    /// Notifies to the user of the elapsed time in the stdout.
    /// </summary>
    /// <param name="sender">Sender of the report.</param>
    /// <param name="session">Session subject of the report.</param>
    private void ProgressEventHandler(object? sender, Session session)
    {
        Logger.LogInformation("Session {SessionId}: {ElapsedTime}", session.Id, session.Elapsed); // TODO: Replace with stdout.
    }
    
    /// <summary>
    /// Notifies to the user in the stdout and with system notifications when
    /// a time is completed.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="timeType">A <see cref="TimeType"/> which represents the time completed.</param>
    private void TimeCompletedHandler(object? sender, TimeType timeType)
    {
        Logger.LogInformation("{TimeType} time was completed.", timeType.ToString()); // TODO: Replace with stdout.
        _notificationManager.CloseLastNotificationAsync();

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
        
        _notificationManager.NotifyAsync(summary, body, NotificationSoundType.TimeCompleted).Forget();
    }

    /// <summary>
    /// Notifies to the user of a second elapsed in the delay
    /// between times.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="time">Elapsed time.</param>
    private void DelayProgressHandler(object? sender, TimeSpan time)
    {
        Logger.LogInformation("Delay: {Delay}", time); // TODO: Replace with stdout.
    }

    /// <summary>
    /// Notifies to the user with the stdout and system notifications when
    /// a session is started.
    /// </summary>
    /// <param name="e">Empty arguments of the invoked event.</param>
    /// <param name="sender">Sender of the event.</param>
    private void SessionStartedHandler(object? sender, EventArgs e)
    {
        _notificationManager.CloseLastNotificationAsync();
        _notificationManager.NotifyAsync(
            summary: _notificationConfig.SessionStartedSummary,
            body: _notificationConfig.SessionStartedBody,
            NotificationSoundType.SessionStarted).Forget();
    }
    
    /// <summary>
    /// Notifies to the user in the stdout and with system notifications when
    /// a session is completed.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="session">The completed <see cref="Session"/>.</param>
    private void SessionCompletedHandler(object? sender, Session session)
    {
        _sessionStorage.AddSession(SessionStatus.Finished, session);
        Logger.LogInformation("Session was completed."); // TODO: Replace with stdout.
        
        _notificationManager.CloseLastNotificationAsync();
        _notificationManager.NotifyAsync(
            summary: _notificationConfig.SessionFinishedSummary,
            body: _notificationConfig.SessionFinishedBody,
            NotificationSoundType.SessionFinished).Forget();
    }
}
