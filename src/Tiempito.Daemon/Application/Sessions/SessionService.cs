using Microsoft.Extensions.Options;

using Tiempito.Daemon.Application.Notifications;
using Tiempito.Daemon.Domain.Notifications.Enums;
using Tiempito.Daemon.Domain.Sessions;
using Tiempito.Daemon.Domain.Sessions.Enums;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.Daemon.Server.Configuration;

namespace Tiempito.Daemon.Application.Sessions;

/// <summary>
/// Service to manage sessions.
/// </summary>
public sealed class SessionService : ISessionService, IAsyncDisposable
{
    private readonly NotificationConfig _notificationConfig;
    private readonly INotificationService _notificationService;
    private readonly IStandardOutQueueWriter _standardOutQueueWriter;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ISessionFactory _sessionFactory;
    private readonly Dictionary<string, Session> _activeSessions;
    private bool _disposed;

    public SessionService(
        ILogger<SessionService> logger,
        IOptions<NotificationConfig> notificationOptions,
        INotificationService notificationService,
        IStandardOutQueueWriter standardOutQueueWriter,
        IHostApplicationLifetime hostApplicationLifetime,
        ISessionFactory sessionFactory,
        Dictionary<string, Session>? activeSessions = null)
    {
        _notificationConfig = notificationOptions.Value;
        _notificationService = notificationService;
        _standardOutQueueWriter = standardOutQueueWriter;
        _hostApplicationLifetime = hostApplicationLifetime;
        _sessionFactory = sessionFactory;
        _activeSessions = activeSessions ?? new Dictionary<string, Session>();
    }

    public async ValueTask<OperationResult> StartSessionAsync(string sessionId = "", string sessionConfigId = "")
    {
        // Try to get the config
        if (!string.IsNullOrWhiteSpace(sessionConfigId) && !_sessionFactory.ExistsConfig(sessionConfigId))
        {
            return new OperationResult(Success: false, Message: $"Session configuration with ID '{sessionConfigId}' was not found");
        }

        // Use session config ID in empty string case
        if (string.IsNullOrWhiteSpace(sessionId))
            sessionId = sessionConfigId;

        // Verify if the session id already exists.
        if (_activeSessions.ContainsKey(sessionId))
            return new OperationResult(Success: false, Message: "There's already a started session with the same ID.");

        var session = _sessionFactory.Create(
            sessionId,
            sessionConfigId,
            OnSessionSecondElapsedAsync,
            OnSessionIntervalCompletedAsync,
            OnSessionCompletedAsync);

        _activeSessions.Add(session.Id, session);
        session.Start(_hostApplicationLifetime.ApplicationStopping);
        await OnSessionStartedAsync(session);
        
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

    public async ValueTask<OperationResult> CancelSessionAsync(string sessionId = "")
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

        await session.CancelAsync();
        return new OperationResult(Success: true, Message: "Session cancelled.");
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        foreach (var session in _activeSessions.Values)
        {
            await session.DisposeAsync();
        }

        _activeSessions.Clear();
    }

    private async Task OnSessionStartedAsync(Session _)
    {
        await _notificationService.CloseLastNotificationAsync();
        await _notificationService.NotifyAsync(
            summary: _notificationConfig.SessionStartedSummary,
            body: _notificationConfig.SessionStartedBody,
            NotificationSoundType.SessionStarted);
    }

    private async ValueTask OnSessionSecondElapsedAsync(Session session)
    {
        var message = $"{session.State.IntervalType.ToString()} time: {session.State.ElapsedTime}";
        await _standardOutQueueWriter.WriteAsync(message);
    }

    private async ValueTask OnSessionIntervalCompletedAsync(Session session)
    {
        if (session.State.IntervalType is SessionIntervalType.Delay)
        {
            return;
        }

        var message = $"{session.State.IntervalType.ToString()} time completed.";
        await _standardOutQueueWriter.WriteAsync(message);

        await _notificationService.CloseLastNotificationAsync();

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

        await _notificationService.NotifyAsync(summary, body, NotificationSoundType.TimeCompleted);
    }

    private async ValueTask OnSessionCompletedAsync(Session session)
    {
        _activeSessions.Remove(session.Id);

        var message = $"Session with id {session.Id} was completed";
        await _standardOutQueueWriter.WriteAsync(message);

        await _notificationService.CloseLastNotificationAsync();
        await _notificationService.NotifyAsync(
            summary: _notificationConfig.SessionFinishedSummary,
            body: _notificationConfig.SessionFinishedBody,
            NotificationSoundType.SessionFinished);
    }
}
