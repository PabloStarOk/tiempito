using Tiempito.Daemon.Server;

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
    private bool _isExiting;
    
    public DaemonWorker(
        IHostApplicationLifetime appLifetime,
        ILogger<DaemonWorker> logger,
        TimeProvider timeProvider,
        IServer server)
    {
        _appLifetime = appLifetime;
        _logger = logger;
        _timeProvider = timeProvider;
        _server = server;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("tiempitod running at: {Time}", _timeProvider.GetUtcNow());

        _server.OnFailed += OnFailedServerHandler;
        await _server.StartAsync(stoppingToken);

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
