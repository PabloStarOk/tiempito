using Tiempitod.NET.Commands.Session;
using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands;

public class CommandHandler : ICommandHandler
{
    private readonly ILogger<CommandHandler> _logger;
    private readonly ISessionManager _sessionManager;
    private CancellationTokenSource _sessionTokenSource;

    public CommandHandler(ILogger<CommandHandler> logger, ISessionManager sessionManager)
    {
        _logger = logger;
        _sessionManager = sessionManager;
        _sessionTokenSource = new CancellationTokenSource();
    }
    
    public async Task HandleCommandAsync(string commandString, CancellationToken stoppingToken)
    {
        ICommand command;
        
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
           
            case "resume":
                command = new ResumeSessionCommand(_sessionManager);
                break;
           
            case "cancel":
                command = new CancelSessionCommand(_sessionManager);
                break;
           
            default:
                _logger.LogError("Unknown command '{givenCommand}'", commandString);
                return;
        }

        await command.ExecuteAsync(_sessionTokenSource.Token);
    }

    public void Dispose()
    {   
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
            return;
        
        if (!_sessionTokenSource.IsCancellationRequested)
            _sessionTokenSource.Cancel();
            
        _sessionTokenSource.Dispose();
    }

    ~CommandHandler()
    {
        Dispose(disposing: false);
    }
}
