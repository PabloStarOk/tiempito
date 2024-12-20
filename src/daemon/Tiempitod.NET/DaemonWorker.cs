using Tiempitod.NET.Session;

namespace Tiempitod.NET;

public class DaemonWorker : BackgroundService
{
    private readonly ILogger<DaemonWorker> _logger;
    private readonly ISessionManager _sessionManager;

    public DaemonWorker(ILogger<DaemonWorker> logger, ISessionManager sessionManager)
    {
        _logger = logger;
        _sessionManager = sessionManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sessionsCompleted = false;
        
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("tiempitod running at: {time}", DateTimeOffset.Now);
            
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!sessionsCompleted)
                await _sessionManager.StartSessionAsync(stoppingToken);

            sessionsCompleted = true;
        }
        
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("tiempitod stopping at: {time}", DateTimeOffset.Now);
    }
}
