namespace Tiempitod.NET.Session;

// TODO: Introduce configuration.
/// <summary>
/// Manages sessions.
/// </summary>
public sealed class SessionManager : ISessionManager
{
    private readonly ILogger<SessionManager> _logger;
    private readonly IProgress<Session> _progress;
    private Session _session;
    
    public SessionManager(ILogger<SessionManager> logger, IProgress<Session> progress) 
    {
        _logger = logger;
        _progress = progress;
    }

    /// <summary>
    /// Starts a session based on the configuration.
    /// </summary>
    public async Task StartSessionAsync(CancellationToken stoppingToken)
    {
        _session = new Session(TimeType.Focus, TimeSpan.Zero, TimeSpan.Zero);
        
        _logger.LogInformation("Starting session at {time}", DateTimeOffset.Now);
        
        for (var i = 0; i < 5; i++)
        {
            if (stoppingToken.IsCancellationRequested)
                break;
            
            await StartCycleAsync(stoppingToken);
        }
        
        if (stoppingToken.IsCancellationRequested)
        {
            _logger.LogWarning("Canceling session at {time}", DateTimeOffset.Now);
            _session.Status = SessionStatus.Stopped;
            return;
        }
        
        _session.Status = SessionStatus.Finished;
        _logger.LogInformation("Finishing session at {time}", DateTimeOffset.Now);
    }

    public void PauseSession()
    {
        _session.Status = SessionStatus.Paused;
        _logger.LogWarning("Pausing session at time {time}", DateTimeOffset.Now);
    }

    public void ContinueSession()
    {
        _session.Status = SessionStatus.Executing;
        _logger.LogWarning("Continuing session at time {time}", DateTimeOffset.Now);
    }
    
    /// <summary>
    /// Starts a cycle that executes one focus time and one break time.
    /// </summary>
    private async Task StartCycleAsync(CancellationToken stoppingToken)
    {
        if (!stoppingToken.IsCancellationRequested)
            await StartTimeAsync(TimeType.Focus, TimeSpan.FromSeconds(30), stoppingToken);
        
        if (!stoppingToken.IsCancellationRequested)
            await StartTimeAsync(TimeType.Break, TimeSpan.FromSeconds(30), stoppingToken);
        
        if (!stoppingToken.IsCancellationRequested)
            _session.CurrentCycle += 1;
    }
    
    private async Task StartTimeAsync(TimeType timeType, TimeSpan duration, CancellationToken stoppingToken)
    {
        _session.CurrentTimeType = timeType;
        _session.Duration = duration;
        _session.Elapsed = TimeSpan.Zero;
        TimeSpan interval = TimeSpan.FromSeconds(1);

        while (_session.Elapsed < duration)
        {
            if (stoppingToken.IsCancellationRequested)
                break;
            
            if (_session.Status is SessionStatus.Paused)
                continue;
            
            await Task.Delay(interval, stoppingToken);
            
            _session.Elapsed += interval; 
            _progress.Report(_session);
            _logger.LogInformation("Time is {elapsed}", _session.Elapsed);
        }
    }
}
