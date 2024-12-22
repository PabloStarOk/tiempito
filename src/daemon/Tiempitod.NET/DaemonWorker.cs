using Tiempitod.NET.Session;

namespace Tiempitod.NET;

using Commands;

/// <summary>
/// Background service of "Tiempito". ⏳
/// </summary>
public class DaemonWorker : BackgroundService
{
    private readonly ILogger<DaemonWorker> _logger;
    private readonly ISessionManager _sessionManager;
    private readonly ICommandServer _commandServer;
    private CancellationTokenSource _sessionTokenSource;

    public DaemonWorker(ILogger<DaemonWorker> logger, ISessionManager sessionManager, ICommandServer commandServer)
    {
        _logger = logger;
        _sessionManager = sessionManager;
        _commandServer = commandServer;
        _sessionTokenSource = new CancellationTokenSource();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("tiempitod running at: {time}", DateTimeOffset.Now);
        
        _commandServer.Start();
        _commandServer.CommandReceived += async (e, args) => await ExecuteSessionCommand(stoppingToken, args);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            
        }

        if (!_sessionTokenSource.IsCancellationRequested)
            await _sessionTokenSource.CancelAsync();
        _sessionTokenSource.Dispose();
        await _commandServer.StopAsync();
        
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("tiempitod stopping at: {time}", DateTimeOffset.Now);
    }
    
    // TODO: Temp function to execute commands.
    private async Task ExecuteSessionCommand(CancellationToken stoppingToken, string command, int secondsDelay = 0)
    {
        int millisecondsDelay = secondsDelay * 1000;
        await Task.Delay(millisecondsDelay, stoppingToken);
        
        switch (command)
        {
            case "start":
                if (_sessionTokenSource.IsCancellationRequested && !_sessionTokenSource.TryReset())
                {
                    _sessionTokenSource.Dispose();
                    _sessionTokenSource = new CancellationTokenSource();
                }
                _sessionTokenSource.Token.ThrowIfCancellationRequested();
                await _sessionManager.StartSessionAsync(_sessionTokenSource.Token);
                break;
            
           case "pause":
               _sessionManager.PauseSession();
               break;
           
           case "continue":
               _sessionManager.ContinueSession();
               break;
           
           case "cancel":
                await _sessionTokenSource.CancelAsync();
                break;
           
           default:
                _logger.LogError("Unknown command '{givenCommand}'", command);
                break;
        }
    }
}
