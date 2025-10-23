using Tiempito.Daemon.Common;
using Tiempito.Daemon.Server.Interfaces;

namespace Tiempito.Daemon;

/// <summary>
/// Background service of "Tiempito". ⏳
/// </summary>
public class DaemonWorker : BackgroundService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ILogger<DaemonWorker> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly IServer _server;
    private readonly IEnumerable<Service> _daemonServices;
    private bool _isExiting;
    
    public DaemonWorker(
        IHostApplicationLifetime appLifetime,
        ILogger<DaemonWorker> logger,
        TimeProvider timeProvider,
        IServer server,
        IEnumerable<Service> daemonServices)
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

        _server.OnFailed += OnFailedServerHandler;
        await _server.StartAsync(stoppingToken);

        foreach (Service service in _daemonServices)
        {
            if (await service.StartServiceAsync())
                continue;

            _logger.LogCritical("Couldn't start a service, daemon exiting. Service: {Service}", service);
            await ExitAsync();
        }

        stoppingToken.Register
        (
            () =>
            {
                if (_isExiting)
                    return;
                
                ThreadPool.QueueUserWorkItem
                (
                    async void (_) =>
                    {
                        try
                        {
                            await ExitAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error occurred while shutting down daemon.");
                        }
                    }
                );
            }
        );
    }

    private async Task ExitAsync()
    {
        if (_isExiting)
            return;

        _isExiting = true;
        
        await _server.StopAsync();
        foreach (Service service in _daemonServices)
        {
            bool stoppedSuccessful = await service.StopServiceAsync();
            if (!stoppedSuccessful)
                _logger.LogCritical("Couldn't stop a service. Service: {Service}", service);
        }
        
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("tiempitod stopped at: {Time}", _timeProvider.GetUtcNow());
        
        _appLifetime.StopApplication();
    }
    
    private async Task OnFailedServerHandler(object? sender, EventArgs e)
    {
        _server.OnFailed -= OnFailedServerHandler;
        await ExitAsync();
    }
}
