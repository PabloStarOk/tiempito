using Tiempitod.NET.Commands.SessionCommands;
using Tiempitod.NET.Server;
using Tiempitod.NET.Server.Messages;
using Tiempitod.NET.Session;

namespace Tiempitod.NET.Commands.Handler;

public class CommandHandler : DaemonService, ICommandHandler
{
    private readonly IServer _server;
    private readonly ISessionManager _sessionManager;
    private CancellationTokenSource _sessionTokenSource;

    public CommandHandler(ILogger<CommandHandler> logger, IServer server, ISessionManager sessionManager) : base(logger)
    {
        _server = server;
        _sessionManager = sessionManager;
        _sessionTokenSource = new CancellationTokenSource();
    }

    protected override void OnStartService()
    {
        _server.RequestReceived += ReceiveRequest;
    }

    protected override void OnStopService()
    {
        _server.RequestReceived -= ReceiveRequest;

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

    private void ReceiveRequest(object? _, Request request)
    {
        HandleCommandAsync(request.Data).GetAwaiter();
    }

    private async Task SendResponseAsync(OperationResult operationResult)
    {
        Response response;

        try
        {
            response = operationResult.Success 
                ? Response.Ok(operationResult.Message) 
                : Response.BadRequest(operationResult.Message);
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Exception captured at {Time}", DateTimeOffset.Now);
            response = Response.InternalError($"An internal error occurred: {ex.Message}");
        }
        
        await _server.SendResponseAsync(response);
    }
}
