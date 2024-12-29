using Microsoft.Extensions.Options;
using Tiempitod.NET.Configuration.Notifications;
using Tiempitod.NET.Configuration.Session;
using Tiempitod.NET.Extensions;
using Tiempitod.NET.Notifications;

namespace Tiempitod.NET.Session;

// TODO: Introduce configuration.
/// <summary>
/// A concrete class to manage sessions.
/// </summary>
public sealed class SessionManager : DaemonService, ISessionManager
{
    private readonly ISessionConfigProvider _sessionConfigProvider;
    private readonly NotificationConfig _notificationConfig;
    private readonly IProgress<Session> _progress;
    private readonly INotificationManager _notificationManager;
    private readonly TimeSpan _interval;
    private Session _currentSession;
    private CancellationTokenSource _timerTokenSource;
    
    public SessionManager(
        ILogger<SessionManager> logger,
        ISessionConfigProvider sessionConfigProvider,
        IOptions<NotificationConfig> notificationOptions,
        INotificationManager notificationManager,
        IProgress<Session> progress) : base(logger)
    {
        _sessionConfigProvider = sessionConfigProvider;
        _notificationConfig = notificationOptions.Value;
        _progress = progress;
        _notificationManager = notificationManager;
        _interval = TimeSpan.FromSeconds(1);
        _timerTokenSource = new CancellationTokenSource();
    }
    
    protected override void OnStopService()
    {
        if (!_timerTokenSource.IsCancellationRequested)
            _timerTokenSource.Cancel();
        
        _timerTokenSource.Dispose();
    }
    
    public OperationResult StartSession(string sessionId = "")
    {
        if (_currentSession.Status is SessionStatus.Executing)
            return new OperationResult(Success: false, Message: "There is already an executed session.");
        
        if (_currentSession.Status is SessionStatus.Paused)
            return new OperationResult(Success: false, Message: "There is already an active paused session.");

        SessionConfig configSessionToUse;

        if (string.IsNullOrWhiteSpace(sessionId))
            configSessionToUse = _sessionConfigProvider.DefaultSessionConfig;
        else if (_sessionConfigProvider.SessionConfigs.TryGetValue(sessionId, out SessionConfig foundSessionConfig))
            configSessionToUse = foundSessionConfig;
        else
            return new OperationResult(Success: false, Message: $"Session with ID {sessionId} was not found");
        
        _currentSession = new Session(
            configSessionToUse.TargetCycles,
            configSessionToUse.FocusDuration,
            configSessionToUse.BreakDuration);
        
        _timerTokenSource = RegenerateTokenSource(_timerTokenSource);
        RunTimerAsync(_timerTokenSource.Token).Forget();
        
        return new OperationResult(Success: true, Message: "Session started.");
    }

    public async Task<OperationResult> PauseSessionAsync()
    {
        if (_currentSession.Status is not SessionStatus.Executing)
            return new OperationResult(Success: false, Message: "There are no running sessions.");
        
        await _timerTokenSource.CancelAsync();
        _currentSession.Status = SessionStatus.Paused;
        Logger.LogWarning("Session paused at time {Time}", DateTimeOffset.Now);
        
        return new OperationResult(Success: true, Message: "Session paused.");
    }

    public OperationResult ResumeSession()
    {
        if (_currentSession.Status is not SessionStatus.Paused)
            return new OperationResult(Success: false, Message: "There are no paused sessions to resume.");
        
        _timerTokenSource = RegenerateTokenSource(_timerTokenSource);
        _currentSession.Status = SessionStatus.Executing;
        RunTimerAsync(_timerTokenSource.Token).Forget();
        Logger.LogWarning("Continuing session at time {Time}", DateTimeOffset.Now);
        
        return new OperationResult(Success: true, Message: "Session resumed.");
    }

    public async Task<OperationResult> CancelSessionAsync()
    {
        if (_currentSession.Status is SessionStatus.Cancelled or SessionStatus.Finished)
            return new OperationResult(Success: false, Message: "There are no active sessions to cancel.");
        
        await _timerTokenSource.CancelAsync();
        
        _currentSession = new Session(
            _sessionConfigProvider.DefaultSessionConfig.TargetCycles,
            _sessionConfigProvider.DefaultSessionConfig.FocusDuration,
            _sessionConfigProvider.DefaultSessionConfig.BreakDuration);
        
        Logger.LogWarning("Session cancelled at {Time}", DateTimeOffset.Now);
        return new OperationResult(Success: true, Message: "Session cancelled.");
    }
    
    /// <summary>
    /// Starts the timer of the session.
    /// </summary>
    /// <param name="stoppingToken">Cancel the timer.</param>
    private async Task RunTimerAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("Starting session at {Time}", DateTimeOffset.Now);
        _currentSession.Status = SessionStatus.Executing;
        await _notificationManager.CloseLastNotificationAsync();
        await _notificationManager.NotifyAsync(
            summary: _notificationConfig.SessionStartedSummary,
            body: _notificationConfig.SessionStartedBody);
        
        while ((_currentSession.CurrentCycle < _currentSession.TargetCycles
               || _currentSession.TargetCycles < 1)
               && !stoppingToken.IsCancellationRequested)
        {
            await StartTimeAsync(stoppingToken);
            await StartTimeAsync(stoppingToken);

            if (stoppingToken.IsCancellationRequested)
                return;
            
            _currentSession.CurrentCycle++;
        }

        if (stoppingToken.IsCancellationRequested)
            return;

        await _notificationManager.CloseLastNotificationAsync();
        await _notificationManager.NotifyAsync(
            summary: _notificationConfig.SessionFinishedSummary,
            body: _notificationConfig.SessionFinishedBody);
        _currentSession.Status = SessionStatus.Finished;
        Logger.LogInformation("Finishing session at {Time}", DateTimeOffset.Now);
    }
    
    /// <summary>
    /// Starts a focus or break time.
    /// </summary>
    /// <param name="stoppingToken">Token to stop the operation.</param>
    private async Task StartTimeAsync(CancellationToken stoppingToken)
    {
        TimeSpan duration = _currentSession.CurrentTimeType is TimeType.Focus 
            ? _currentSession.FocusDuration 
            : _currentSession.BreakDuration;

        while (_currentSession.Elapsed < duration && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch
            {
                return;
            }


            if (stoppingToken.IsCancellationRequested)
                return;

            _currentSession.Elapsed += _interval;
            _progress.Report(_currentSession);
            Logger.LogInformation("Time is {Elapsed}", _currentSession.Elapsed);
        }

        if (stoppingToken.IsCancellationRequested)
            return;

        await _notificationManager.CloseLastNotificationAsync();
        string summary;
        string body;
        if (_currentSession.CurrentTimeType is TimeType.Focus)
        {
            summary = _notificationConfig.FocusCompletedSummary;
            body = _notificationConfig.FocusCompletedBody;
            _currentSession.CurrentTimeType = TimeType.Break;
        }
        else
        {
            summary = _notificationConfig.BreakCompletedSummary;
            body = _notificationConfig.BreakCompletedBody;
            _currentSession.CurrentTimeType = TimeType.Focus;   
        }
        await _notificationManager.NotifyAsync(summary, body);
        
        _currentSession.Elapsed = TimeSpan.Zero;
    }
}
