using Tiempitod.NET.Notifications;

namespace Tiempitod.NET.Session;

// TODO: Introduce configuration.
/// <summary>
/// A concrete class to manage sessions.
/// </summary>
public sealed class SessionManager : DaemonService, ISessionManager
{
    private readonly IProgress<Session> _progress;
    private readonly INotificationManager _notificationManager;
    private readonly TimeSpan _interval;
    private Session _session;
    private CancellationTokenSource _timerTokenSource;
    
    public SessionManager(ILogger<SessionManager> logger, INotificationManager notificationManager, IProgress<Session> progress) : base(logger)
    {
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
    
    public OperationResult StartSession(CancellationToken daemonStoppingToken)
    {
        if (_session.Status is SessionStatus.Executing)
            return new OperationResult(Success: false, Message: "There is already an executed session.");
        
        if (_session.Status is SessionStatus.Paused)
            return new OperationResult(Success: false, Message: "There is already an active paused session.");

        _session = new Session(
            TimeType.Focus,
            elapsed: TimeSpan.Zero,
            focusDuration: TimeSpan.FromSeconds(30),
            breakDuration: TimeSpan.FromSeconds(30),
            targetCycles: 5);

        RegenerateCancellationToken();
        RunTimerAsync(_timerTokenSource.Token).Forget();
        
        return new OperationResult(Success: true, Message: "Session started.");
    }

    public async Task<OperationResult> PauseSessionAsync()
    {
        if (_session.Status is not SessionStatus.Executing)
            return new OperationResult(Success: false, Message: "There are no running sessions.");
        
        await _timerTokenSource.CancelAsync();
        _session.Status = SessionStatus.Paused;
        Logger.LogWarning("Session paused at time {time}", DateTimeOffset.Now);
        
        return new OperationResult(Success: true, Message: "Session paused.");
    }

    public OperationResult ResumeSession()
    {
        if (_session.Status is not SessionStatus.Paused)
            return new OperationResult(Success: false, Message: "There are no paused sessions to resume.");
        
        RegenerateCancellationToken();
        _session.Status = SessionStatus.Executing;
        RunTimerAsync(_timerTokenSource.Token).Forget();
        Logger.LogWarning("Continuing session at time {time}", DateTimeOffset.Now);
        
        return new OperationResult(Success: true, Message: "Session resumed.");
    }

    public async Task<OperationResult> CancelSessionAsync()
    {
        if (_session.Status is SessionStatus.Cancelled or SessionStatus.Finished)
            return new OperationResult(Success: false, Message: "There are no active sessions to cancel.");
        
        await _timerTokenSource.CancelAsync();
        _session = new Session
        {
            Status = SessionStatus.Cancelled
        };
        Logger.LogWarning("Session cancelled at {time}", DateTimeOffset.Now);
        return new OperationResult(Success: true, Message: "Session cancelled.");
    }

    private void RegenerateCancellationToken()
    {
        if (_timerTokenSource.TryReset())
            return;
        
        _timerTokenSource.Dispose();
        _timerTokenSource = new CancellationTokenSource();
    }
    
    /// <summary>
    /// Starts the timer of the session.
    /// </summary>
    /// <param name="stoppingToken">Cancel the timer.</param>
    private async Task RunTimerAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("Starting session at {time}", DateTimeOffset.Now);
        _session.Status = SessionStatus.Executing;
        await _notificationManager.NotifySessionStartedAsync();
        
        while (_session.CurrentCycle < _session.TargetCycles && !stoppingToken.IsCancellationRequested)
        {
            await StartTimeAsync(stoppingToken);
            await StartTimeAsync(stoppingToken);

            if (stoppingToken.IsCancellationRequested)
                return;
            
            _session.CurrentCycle++;
        }

        if (stoppingToken.IsCancellationRequested)
            return;

        await _notificationManager.NotifySessionFinishedAsync();
        _session.Status = SessionStatus.Finished;
        Logger.LogInformation("Finishing session at {time}", DateTimeOffset.Now);
    }
    
    /// <summary>
    /// Starts a focus or break time.
    /// </summary>
    /// <param name="stoppingToken">Token to stop the operation.</param>
    private async Task StartTimeAsync(CancellationToken stoppingToken)
    {
        TimeSpan duration = _session.CurrentTimeType is TimeType.Focus 
            ? _session.FocusDuration 
            : _session.BreakDuration;

        while (_session.Elapsed < duration && !stoppingToken.IsCancellationRequested)
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

            _session.Elapsed += _interval;
            _progress.Report(_session);
            Logger.LogInformation("Time is {elapsed}", _session.Elapsed);
        }

        if (stoppingToken.IsCancellationRequested)
            return;

        if (_session.CurrentTimeType is TimeType.Focus)
        {
            await _notificationManager.NotifyFocusTimeCompletedAsync();
            _session.CurrentTimeType = TimeType.Break;
        }
        else
        {
            await _notificationManager.NotifyBreakTimeCompletedAsync();
            _session.CurrentTimeType = TimeType.Focus;
        }
        
        _session.Elapsed = TimeSpan.Zero;
    }
}
