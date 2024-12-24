namespace Tiempitod.NET;

/// <summary>
/// Background service of "Tiempito". ⏳
/// </summary>
public class DaemonWorker : BackgroundService
{
    private readonly ILogger<DaemonWorker> _logger;
    private readonly IEnumerable<DaemonService> _daemonServices;
    private bool _startedSuccessfully;

    public DaemonWorker(ILogger<DaemonWorker> logger, IEnumerable<DaemonService> daemonServices)
    {
        _logger = logger;
        _daemonServices = daemonServices;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("tiempitod running at: {time}", DateTimeOffset.Now);

        _startedSuccessfully = true;
        
        foreach (DaemonService service in _daemonServices)
        {
            if (!service.StartService())
                _startedSuccessfully = false;
        }
        
        while (!stoppingToken.IsCancellationRequested && _startedSuccessfully) ;
        
        foreach (DaemonService service in _daemonServices)
            service.StopService();
        
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("tiempitod stopped at: {time}", DateTimeOffset.Now);
    }
}
