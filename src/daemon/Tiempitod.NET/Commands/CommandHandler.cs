using Tiempitod.NET.Commands.Session;
using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands;

public class CommandHandler : DaemonService, ICommandHandler
{
    private readonly ICommandServer _commandServer;
    private readonly ISessionManager _sessionManager;
    private CancellationTokenSource _sessionTokenSource;

    public CommandHandler(ILogger<CommandHandler> logger, ICommandServer commandServer, ISessionManager sessionManager) : base(logger)
    {
        _commandServer = commandServer;
        _sessionManager = sessionManager;
        _sessionTokenSource = new CancellationTokenSource();
    }

    protected override void OnStartService()
    {
        _commandServer.CommandReceived += ReceiveCommand;
    }

    protected override void OnStopService()
    {
        _commandServer.CommandReceived -= ReceiveCommand;

        if (!_sessionTokenSource.IsCancellationRequested)
            _sessionTokenSource.Cancel();

        _sessionTokenSource.Dispose();
    }
    
    public async Task HandleCommandAsync(string commandString)
    {
        ICommand command;
        OperationResult operationResult;
        
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
                operationResult = new OperationResult
                (
                    Success: false,
                    Message: $"Unknown command '{commandString}'"
                );
                await SendResponseAsync(operationResult);
                return;
        }

        operationResult = await command.ExecuteAsync(_sessionTokenSource.Token);
        await SendResponseAsync(operationResult);
    }

    private void ReceiveCommand(object? _, string command)
    {
        HandleCommandAsync(command).GetAwaiter();
    }

    private async Task SendResponseAsync(OperationResult operationResult)
    {
        DaemonResponse daemonResponse;

        try
        {
            daemonResponse = operationResult.Success 
                ? DaemonResponse.Ok(operationResult.Message) 
                : DaemonResponse.BadRequest(operationResult.Message);
        }
        catch (Exception ex)
        {
            Logger.LogCritical("Exception captured at {time}, error: {error}", DateTimeOffset.Now, ex.Message);
            daemonResponse = DaemonResponse.InternalError($"An internal error occurred: {ex.Message}");
        }
        
        await _commandServer.SendResponseAsync(daemonResponse);
    }
}
