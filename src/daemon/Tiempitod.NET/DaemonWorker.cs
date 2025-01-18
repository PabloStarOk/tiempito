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

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("tiempitod running at: {Time}", DateTimeOffset.Now);
        
        foreach (DaemonService service in _daemonServices)
        {
            if (service.StartService())
                continue;
            
            _logger.LogCritical("Couldn't start a service, daemon exiting. Service: {Service}", service);
                
            Exit(1);
            return Task.CompletedTask;
        }
        
        while (!stoppingToken.IsCancellationRequested) ;
        
        Exit(0);
        return Task.CompletedTask;
    }

    private void Exit(int exitCode)
    {
        foreach (DaemonService service in _daemonServices)
        {
            if (!service.StopService())
                _logger.LogCritical("Couldn't stop a service. Service: {Service}", service);
        }
        
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("tiempitod stopped at: {Time}", DateTimeOffset.Now);
        
        Environment.Exit(exitCode);
    }
}
