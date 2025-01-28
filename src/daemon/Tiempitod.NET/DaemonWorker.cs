using Tiempitod.NET.Server;

namespace Tiempitod.NET;

/// <summary>
/// Background service of "Tiempito". ⏳
/// </summary>
public class DaemonWorker : BackgroundService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ILogger<DaemonWorker> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly IServer _server;
    private readonly IEnumerable<DaemonService> _daemonServices;
    private bool _isExiting;
    
    public DaemonWorker(
        IHostApplicationLifetime appLifetime,
        ILogger<DaemonWorker> logger,
        TimeProvider timeProvider,
        IServer server,
        IEnumerable<DaemonService> daemonServices)
    {
        _appLifetime = appLifetime;
        _logger = logger;
        _timeProvider = timeProvider;
        _server = server;
        _daemonServices = daemonServices;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("tiempitod running at: {Time}", _timeProvider.GetUtcNow());

        _server.OnFailed += ServerOnFailedHandler; 
        await _server.StartAsync(stoppingToken);
        
        foreach (DaemonService service in _daemonServices)
        {
            if (service.StartService())
                continue;
            
            _logger.LogCritical("Couldn't start a service, daemon exiting. Service: {Service}", service);
            Exit();
        }

        stoppingToken.Register(Exit);
    }

    private void Exit()
    {
        if (_isExiting)
            return;

        _isExiting = true;

        _server.StopAsync();
        foreach (DaemonService service in _daemonServices)
        {
            if (!service.StopService())
                _logger.LogCritical("Couldn't stop a service. Service: {Service}", service);
        }
        
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("tiempitod stopped at: {Time}", _timeProvider.GetUtcNow());
        
        _appLifetime.StopApplication();
    }

    private void ServerOnFailedHandler(object? sender, EventArgs e)
    {
        _server.OnFailed -= ServerOnFailedHandler;
        Exit();
    }
}
