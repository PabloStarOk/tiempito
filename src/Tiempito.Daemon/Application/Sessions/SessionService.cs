using Tiempito.Daemon.Application.Notifications;
using Tiempito.Daemon.Domain.Notifications.Enums;
using Tiempito.Daemon.Domain.Sessions;
using Tiempito.Daemon.Domain.Sessions.Enums;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models;

namespace Tiempito.Daemon.Application.Sessions;

/// <summary>
/// Service to manage sessions.
/// </summary>
public sealed class SessionService : ISessionService, IDisposable
{
    private readonly INotificationService _notificationService;
    private readonly IStandardOutQueueWriter _standardOutQueueWriter;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ISessionFactory _sessionFactory;
    private readonly Dictionary<string, ISession> _activeSessions;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionService"/> class.
    /// </summary>
    /// <param name="notificationService">Service for sending notifications.</param>
    /// <param name="standardOutQueueWriter">Writer for standard output queue.</param>
    /// <param name="hostApplicationLifetime">Application lifetime manager.</param>
    /// <param name="sessionFactory">Factory for creating sessions.</param>
    /// <param name="activeSessions">Optional dictionary of active sessions.</param>
    public SessionService(
        INotificationService notificationService,
        IStandardOutQueueWriter standardOutQueueWriter,
        IHostApplicationLifetime hostApplicationLifetime,
        ISessionFactory sessionFactory,
        Dictionary<string, ISession>? activeSessions = null)
    {
        _notificationService = notificationService;
        _standardOutQueueWriter = standardOutQueueWriter;
        _hostApplicationLifetime = hostApplicationLifetime;
        _sessionFactory = sessionFactory;
        _activeSessions = activeSessions ?? new Dictionary<string, ISession>();
    }

    /// <inheritdoc/>
    public async ValueTask<OperationResult> StartSessionAsync(string sessionId = "", string sessionConfigId = "")
    {
        // Try to get the config
        if (!string.IsNullOrWhiteSpace(sessionConfigId) && !_sessionFactory.ExistsConfig(sessionConfigId))
        {
            return new OperationResult(Success: false, Message: $"Session configuration with ID '{sessionConfigId}' was not found");
        }

        // Use session config ID in empty string case
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            sessionId = sessionConfigId;
        }

        // Verify if the session id already exists.
        if (_activeSessions.ContainsKey(sessionId))
        {
            return new OperationResult(Success: false, Message: "There's already a started session with the same ID.");
        }

        var session = _sessionFactory.Create(sessionId, sessionConfigId);
        session.SecondElapsedAsync += OnSessionSecondElapsedAsync;
        session.IntervalCompletedAsync += OnSessionIntervalCompletedAsync;
        session.CompletedAsync += OnSessionCompletedAsync;
        _activeSessions.Add(session.Id, session);
        session.Start(_hostApplicationLifetime.ApplicationStopping);
        await OnSessionStartedAsync(session);

        return new OperationResult(Success: true, Message: "Session started.");
    }

    /// <inheritdoc/>
    public OperationResult PauseSession(string sessionId = "")
    {
        var executingSessions = _activeSessions
            .Where(s => s.Value.State.Status is SessionStatus.Executing)
            .ToDictionary(s => s.Key, s => s.Value);

        if (executingSessions.Count < 1)
        {
            return new OperationResult(Success: false, Message: "There are no running sessions to pause.");
        }

        ISession? session = null;
        if (!string.IsNullOrWhiteSpace(sessionId) && !executingSessions.TryGetValue(sessionId, out session))
        {
            return new OperationResult(Success: false, Message: $"Running session with ID '{sessionId}' was not found.");
        }

        if (string.IsNullOrWhiteSpace(sessionId) || session is null)
        {
            session = executingSessions.First().Value;
        }

        session.Pause();

        return new OperationResult(Success: true, Message: "Session paused.");
    }

    /// <inheritdoc/>
    public OperationResult ResumeSession(string sessionId = "")
    {
        var pausedSessions = _activeSessions
            .Where(s => s.Value.State.Status is SessionStatus.Paused)
            .ToDictionary(s => s.Key, s => s.Value);

        if (pausedSessions.Count < 1)
        {
            return new OperationResult(Success: false, Message: "There are no paused sessions to resume.");
        }

        ISession? session = null;
        if (!string.IsNullOrWhiteSpace(sessionId) && !pausedSessions.TryGetValue(sessionId, out session))
        {
            return new OperationResult(Success: false, Message: $"Paused session with ID '{sessionId}' was not found.");
        }

        if (string.IsNullOrWhiteSpace(sessionId) || session is null)
        {
            session = pausedSessions.First().Value;
        }

        session.Resume();
        return new OperationResult(Success: true, Message: "Session resumed.");
    }

    /// <inheritdoc/>
    public OperationResult CancelSession(string sessionId = "")
    {
        if (_activeSessions.Count < 1)
        {
            return new OperationResult(Success: false, Message: "There are no sessions to cancel.");
        }

        ISession? session = null;
        if (!string.IsNullOrWhiteSpace(sessionId) && !_activeSessions.Remove(sessionId, out session))
        {
            return new OperationResult(Success: false, Message: $"Started session with ID '{sessionId}' was not found.");
        }

        if (string.IsNullOrWhiteSpace(sessionId) || session is null)
        {
            session = _activeSessions.First().Value;
            _activeSessions.Remove(session.Id);
        }

        session.Cancel();
        session.Dispose();
        return new OperationResult(Success: true, Message: "Session cancelled.");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        foreach (var session in _activeSessions.Values)
        {
            session.Cancel();
            session.Dispose();
            session.SecondElapsedAsync -= OnSessionSecondElapsedAsync;
            session.IntervalCompletedAsync -= OnSessionIntervalCompletedAsync;
            session.CompletedAsync -= OnSessionCompletedAsync;
        }

        _activeSessions.Clear();
    }

    private static SessionProgressMessage CreateSessionProgressMessage(
        ISession session,
        bool intervalCompleted = false,
        bool sessionCompleted = false)
    {
        IPC.Models.Enums.SessionIntervalType intervalType = session.State.IntervalType switch
        {
            SessionIntervalType.Focus => IPC.Models.Enums.SessionIntervalType.Focus,
            SessionIntervalType.Break => IPC.Models.Enums.SessionIntervalType.Break,
            SessionIntervalType.Delay => IPC.Models.Enums.SessionIntervalType.Delay,
            _ => throw new InvalidOperationException("Unknown session interval type.")
        };

        return SessionProgressMessage.CreateNew(
            sessionId: session.Id,
            intervalDuration: session.State.TargetDuration,
            intervalType: intervalType,
            cycle: session.State.Cycle,
            elapsedTime: session.State.ElapsedTime,
            intervalCompleted: intervalCompleted,
            sessionCompleted: sessionCompleted);
    }

    private async Task OnSessionStartedAsync(ISession session)
    {
        await _notificationService.NotifyAsync(session.State, NotificationType.SessionStarted);
    }

    private async ValueTask OnSessionSecondElapsedAsync(ISession session)
    {
        var message = CreateSessionProgressMessage(session);
        await _standardOutQueueWriter.WriteAsync(message);
    }

    private async ValueTask OnSessionIntervalCompletedAsync(ISession session)
    {
        if (session.State.IntervalType is SessionIntervalType.Delay)
        {
            return;
        }

        var message = CreateSessionProgressMessage(session, intervalCompleted: true);
        await _standardOutQueueWriter.WriteAsync(message);
        await _notificationService.NotifyAsync(session.State, NotificationType.SessionIntervalCompleted);
    }

    private async ValueTask OnSessionCompletedAsync(ISession session)
    {
        _activeSessions.Remove(session.Id);
        session.Dispose();
        var message = CreateSessionProgressMessage(session, sessionCompleted: true);
        await _standardOutQueueWriter.WriteAsync(message);
        await _notificationService.NotifyAsync(session.State, NotificationType.SessionCompleted);
    }
}
