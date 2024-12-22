namespace Tiempitod.NET.Session;

/// <summary>
/// Manages sessions.
/// </summary>
public sealed class SessionManager : ISessionManager
{
    private readonly ILogger<SessionManager> _logger;
    private readonly IProgress<SessionProgress> _progress;
    
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
        _logger.LogInformation("Starting session at {time}", DateTimeOffset.Now);
        
        // TODO: Replace cycles with configuration.
        for (var i = 0; i < 5; i++)
            await StartCycleAsync(stoppingToken);
        
        _logger.LogInformation("Finishing session at {time}", DateTimeOffset.Now);
    }
    
    /// <summary>
    /// Starts a cycle that executes one focus time and one break time.
    /// </summary>
    private async Task StartCycleAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting a cycle at {time}", DateTimeOffset.Now);
        // TODO: Replace duration with configuration.
        await StartTimeAsync(TimeType.Focus, TimeSpan.FromSeconds(5), stoppingToken);
        await StartTimeAsync(TimeType.Break, TimeSpan.FromSeconds(5), stoppingToken);
        _logger.LogInformation("A cycle was completed at {time}", DateTimeOffset.Now);
    }
    
    private async Task StartTimeAsync(TimeType timeType, TimeSpan duration, CancellationToken stoppingToken)
    {
        TimeSpan elapsed = TimeSpan.Zero;
        TimeSpan interval = TimeSpan.FromSeconds(1);
        
        _logger.LogInformation("Starting {timeType} time at {time}", timeType, DateTimeOffset.Now);

        while (elapsed < duration)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning("Stopping {timeType} time at {time}", timeType, DateTimeOffset.Now);
                break;
            }
        
            await Task.Delay(interval, stoppingToken);
            
            elapsed += interval;
            _progress.Report(new SessionProgress(timeType, elapsed, duration));
        }

        _logger.LogInformation("A {timeType} time finished at {time}", timeType, DateTimeOffset.Now);
    }
}
