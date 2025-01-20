namespace Tiempitod.NET;

/// <summary>
/// Background service of "Tiempito". ⏳
/// </summary>
public class DaemonWorker : BackgroundService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ILogger<DaemonWorker> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly IEnumerable<DaemonService> _daemonServices;
    private bool _isExiting;
    
    public DaemonWorker(IHostApplicationLifetime appLifetime, ILogger<DaemonWorker> logger, TimeProvider timeProvider, IEnumerable<DaemonService> daemonServices)
    {
        _appLifetime = appLifetime;
        _logger = logger;
        _timeProvider = timeProvider;
        _daemonServices = daemonServices;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("tiempitod running at: {Time}", _timeProvider.GetUtcNow());
        
        foreach (DaemonService service in _daemonServices)
        {
            if (service.StartService())
                continue;
            
            _logger.LogCritical("Couldn't start a service, daemon exiting. Service: {Service}", service);
            Exit();
            return Task.CompletedTask;
        }

        stoppingToken.Register(Exit);
        return Task.CompletedTask;
    }

    private void Exit()
    {
        if (_isExiting)
            return;

        _isExiting = true;
        
        foreach (DaemonService service in _daemonServices)
        {
            if (!service.StopService())
                _logger.LogCritical("Couldn't stop a service. Service: {Service}", service);
        }
        
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("tiempitod stopped at: {Time}", _timeProvider.GetUtcNow());
        
        _appLifetime.StopApplication();
    }
}
