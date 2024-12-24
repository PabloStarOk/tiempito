namespace Tiempitod.NET.Session;

// TODO: Introduce configuration.
/// <summary>
/// A concrete class to manage sessions.
/// </summary>
public sealed class SessionManager : DaemonService, ISessionManager
{
    private readonly IProgress<Session> _progress;
    private readonly TimeSpan _interval;
    private Session _session;
    private CancellationTokenSource _timerTokenSource;
    
    public SessionManager(ILogger<SessionManager> logger, IProgress<Session> progress) : base(logger)
    {
        _progress = progress;
        _interval = TimeSpan.FromSeconds(1);
        _timerTokenSource = new CancellationTokenSource();
    }

    protected override void OnStopService()
    {
        if (!_timerTokenSource.IsCancellationRequested)
            _timerTokenSource.Cancel();
        
        _timerTokenSource.Dispose();
    }
    
    public void StartSession(CancellationToken daemonStoppingToken)
    {
        _session = new Session(
            TimeType.Focus,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(30),
            5);

        RegenerateCancellationToken();
        RunTimerAsync(_timerTokenSource.Token).Forget();
    }

    public async Task PauseSessionAsync()
    {
        await _timerTokenSource.CancelAsync();
        _session.Status = SessionStatus.Paused;
        Logger.LogWarning("Session paused at time {time}", DateTimeOffset.Now);
    }

    public void ResumeSession()
    {
        RegenerateCancellationToken();
        _session.Status = SessionStatus.Executing;
        RunTimerAsync(_timerTokenSource.Token).Forget();
        Logger.LogWarning("Continuing session at time {time}", DateTimeOffset.Now);
    }

    public async Task CancelSessionAsync()
    {
        await _timerTokenSource.CancelAsync();
        _session = new Session
        {
            Status = SessionStatus.Cancelled
        };
        Logger.LogWarning("Session cancelled at {time}", DateTimeOffset.Now);
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

        _session.CurrentTimeType = _session.CurrentTimeType is TimeType.Focus 
            ? TimeType.Break 
            : TimeType.Focus;
        
        _session.Elapsed = TimeSpan.Zero;
    }
}
