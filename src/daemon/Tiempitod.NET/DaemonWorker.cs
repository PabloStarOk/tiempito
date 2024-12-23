using Tiempitod.NET.Commands;

namespace Tiempitod.NET;

/// <summary>
/// Background service of "Tiempito". ⏳
/// </summary>
public class DaemonWorker : BackgroundService
{
    private readonly ILogger<DaemonWorker> _logger;
    private readonly ICommandListener _commandListener;
    private readonly ICommandHandler _commandHandler;

    public DaemonWorker(ILogger<DaemonWorker> logger, ICommandListener commandListener, ICommandHandler commandHandler)
    {
        _logger = logger;
        _commandListener = commandListener;
        _commandHandler = commandHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("tiempitod running at: {time}", DateTimeOffset.Now);
        
        _commandListener.CommandReceived += async (_, command) => await _commandHandler.HandleCommandAsync(command, stoppingToken);
        _commandListener.Start();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            
        }
        
        await _commandListener.StopAsync();
        _commandHandler.Dispose();
        
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("tiempitod stopping at: {time}", DateTimeOffset.Now);
    }
}
