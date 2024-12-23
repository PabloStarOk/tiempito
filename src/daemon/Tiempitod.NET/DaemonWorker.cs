using Tiempitod.NET.Commands;

namespace Tiempitod.NET;

/// <summary>
/// Background service of "Tiempito". ⏳
/// </summary>
public class DaemonWorker : BackgroundService
{
    private readonly ILogger<DaemonWorker> _logger;
    private readonly ICommandServer _commandServer;
    private readonly ICommandHandler _commandHandler;

    public DaemonWorker(ILogger<DaemonWorker> logger, ICommandServer commandServer, ICommandHandler commandHandler)
    {
        _logger = logger;
        _commandServer = commandServer;
        _commandHandler = commandHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("tiempitod running at: {time}", DateTimeOffset.Now);
        
        _commandServer.CommandReceived += async (_, command) => await _commandHandler.HandleCommandAsync(command, stoppingToken);
        _commandServer.Start();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            
        }
        
        await _commandServer.StopAsync();
        _commandHandler.Dispose();
        
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("tiempitod stopping at: {time}", DateTimeOffset.Now);
    }
}
