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
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("tiempitod running at: {time}", DateTimeOffset.Now);

        using var cancellationTokenSource = new CancellationTokenSource();
        
        Task sessionTask =  ExecuteSessionCommand(cancellationTokenSource, "start");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            if (sessionTask.IsCompleted)
                sessionTask.Dispose();
        }

        await cancellationTokenSource.CancelAsync();
        
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("tiempitod stopping at: {time}", DateTimeOffset.Now);
    }
    
    // TODO: Temp function to execute commands.
    private async Task ExecuteSessionCommand(CancellationTokenSource tokenSource, string command, int secondsDelay = 0)
    {
        int millisecondsDelay = secondsDelay * 1000;
        await Task.Delay(millisecondsDelay, tokenSource.Token);
        
        switch (command)
        {
            case "start":
                tokenSource.Token.ThrowIfCancellationRequested();
                await _sessionManager.StartSessionAsync(tokenSource.Token);
                break;
            
           case "pause":
               _sessionManager.PauseSession();
               break;
           
           case "continue":
               _sessionManager.ContinueSession();
               break;
           
           case "cancel":
                await tokenSource.CancelAsync();
                break;
           
           default:
                _logger.LogError("Unknown command '{givenCommand}'", command);
                break;
        }
    }
}
