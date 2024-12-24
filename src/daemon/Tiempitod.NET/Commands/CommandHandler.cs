using Tiempitod.NET.Commands.Session;
using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands;

public class CommandHandler : DaemonService, ICommandHandler
{
    private readonly ICommandListener _commandListener;
    private readonly ISessionManager _sessionManager;
    private CancellationTokenSource _sessionTokenSource;

    public CommandHandler(ILogger<CommandHandler> logger, ICommandListener commandListener, ISessionManager sessionManager) : base(logger)
    {
        _commandListener = commandListener;
        _sessionManager = sessionManager;
        _sessionTokenSource = new CancellationTokenSource();
    }

    protected override void OnStartService()
    {
        _commandListener.CommandReceived += ReceiveCommand;
    }

    protected override void OnStopService()
    {
        _commandListener.CommandReceived -= ReceiveCommand;

        if (!_sessionTokenSource.IsCancellationRequested)
            _sessionTokenSource.Cancel();

        _sessionTokenSource.Dispose();
    }
    
    public async Task HandleCommandAsync(string commandString)
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
                Logger.LogError("Unknown command '{givenCommand}'", commandString);
                return;
        }

        await command.ExecuteAsync(_sessionTokenSource.Token);
    }

    private void ReceiveCommand(object? _, string command)
    {
        HandleCommandAsync(command).GetAwaiter();
    }
}
