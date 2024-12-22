namespace Tiempitod.NET.Session;

// TODO: Introduce configuration.
/// <summary>
/// Manages sessions.
/// </summary>
public sealed class SessionManager : ISessionManager
{
    private readonly ILogger<SessionManager> _logger;
    private readonly IProgress<SessionProgress> _progress;
    private SessionProgress _sessionProgress;
    
    public SessionManager(ILogger<SessionManager> logger, IProgress<SessionProgress> progress) 
    {
        _logger = logger;
        _progress = progress;
    }

    /// <summary>
    /// Starts a session based on the configuration.
    /// </summary>
    public async Task StartSessionAsync(CancellationToken stoppingToken)
    {
        _sessionProgress = new SessionProgress(TimeType.Focus, TimeSpan.Zero, TimeSpan.Zero);
        
        _logger.LogInformation("Starting session at {time}", DateTimeOffset.Now);
        
        for (var i = 0; i < 5; i++)
        {
            if (stoppingToken.IsCancellationRequested)
                break;
            
            await StartCycleAsync(stoppingToken);
        }
        
        if (stoppingToken.IsCancellationRequested)
        {
            _logger.LogWarning("Stopping session at {time}", DateTimeOffset.Now);
            _sessionProgress.Status = SessionStatus.Stopped;
            return;
        }
        
        _sessionProgress.Status = SessionStatus.Finished;
        _logger.LogInformation("Finishing session at {time}", DateTimeOffset.Now);
    }

    public void PauseSession()
    {
        _sessionProgress.Status = SessionStatus.Paused;
        _logger.LogWarning("Pausing session at time {time}", DateTimeOffset.Now);
    }

    public void ContinueSession()
    {
        _sessionProgress.Status = SessionStatus.Executing;
        _logger.LogWarning("Continuing session at time {time}", DateTimeOffset.Now);
    }
    
    /// <summary>
    /// Starts a cycle that executes one focus time and one break time.
    /// </summary>
    private async Task StartCycleAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting a cycle at {time}", DateTimeOffset.Now);
        
        if (!stoppingToken.IsCancellationRequested)
            await StartTimeAsync(TimeType.Focus, TimeSpan.FromSeconds(30), stoppingToken);
        
        if (!stoppingToken.IsCancellationRequested)
            await StartTimeAsync(TimeType.Break, TimeSpan.FromSeconds(30), stoppingToken);
        
        if (stoppingToken.IsCancellationRequested)
            return;
        
        _logger.LogInformation("A cycle was completed at {time}", DateTimeOffset.Now);
        _sessionProgress.CurrentCycle += 1;
    }
    
    private async Task StartTimeAsync(TimeType timeType, TimeSpan duration, CancellationToken stoppingToken)
    {
        _sessionProgress.TimeType = timeType;
        _sessionProgress.Duration = duration;
        _sessionProgress.Elapsed = TimeSpan.Zero;
        TimeSpan interval = TimeSpan.FromSeconds(1);
        
        _logger.LogInformation("Starting {timeType} time at {time}", timeType, DateTimeOffset.Now);

        while (_sessionProgress.Elapsed < duration)
        {
            if (stoppingToken.IsCancellationRequested)
                break;
            
            if (_sessionProgress.Status is SessionStatus.Paused)
                continue;
            
            await Task.Delay(interval, stoppingToken);
            
            _sessionProgress.Elapsed += interval; 
            _progress.Report(_sessionProgress);
            _logger.LogInformation("Time is {elapsed}", _sessionProgress.Elapsed);
        }
        
        if (stoppingToken.IsCancellationRequested)
        {
            _logger.LogWarning("Stopping {timeType} time at {time}", timeType, DateTimeOffset.Now);
            return;
        }
        
        _logger.LogInformation("A {timeType} time finished at {time}", timeType, DateTimeOffset.Now);
    }
}
