using Tiempitod.NET.Session;
using Tiempitod.NET.Commands;
using Tiempitod.NET.Commands.Session;

namespace Tiempitod.NET;

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
    private async Task ExecuteSessionCommand(CancellationToken stoppingToken, string commandString, int secondsDelay = 0)
    {
        ICommand command;
        int millisecondsDelay = secondsDelay * 1000;
        await Task.Delay(millisecondsDelay, stoppingToken);
        
        switch (commandString)
        {
            case "start":
                if (_sessionTokenSource.IsCancellationRequested && !_sessionTokenSource.TryReset())
                {
                    _sessionTokenSource.Dispose();
                    _sessionTokenSource = new CancellationTokenSource();
                }
                _sessionTokenSource.Token.ThrowIfCancellationRequested();
                command = new StartSessionCommand(_sessionManager);
                break;
            
           case "pause":
               command = new PauseSessionCommand(_sessionManager);
               break;
           
           case "continue":
               command = new ContinueSessionCommand(_sessionManager);
               break;
           
           case "cancel":
                command = new CancelSessionCommand(_sessionTokenSource);
                break;
           
           default:
                _logger.LogError("Unknown command '{givenCommand}'", commandString);
                return;
        }

        await command.ExecuteAsync(_sessionTokenSource.Token);
    }
}
