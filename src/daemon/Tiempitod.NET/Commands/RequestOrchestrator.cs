using Tiempito.IPC.NET.Messages;
using Tiempitod.NET.Extensions;
using Tiempitod.NET.Server;

namespace Tiempitod.NET.Commands;

/// <summary>
/// Orchestrates the requests received by the server to execute commands.
/// </summary>
public class RequestOrchestrator : DaemonService
{
    private readonly IServer _server;
    private readonly ICommandHandlerFactory _commandHandlerFactory;
    private readonly EventHandler<Request> _eventHandler;
    private readonly CancellationTokenSource _sessionTokenSource;
    
    /// <summary>
    /// Instantiates a new <see cref="RequestOrchestrator"/>.
    /// </summary>
    /// <param name="logger">Logger to log special events.</param>
    /// <param name="server">Server that receives requests.</param>
    /// <param name="commandHandlerFactory">Factory to create command handlers.</param>
    public RequestOrchestrator(
        ILogger<RequestOrchestrator> logger,
        IServer server,
        ICommandHandlerFactory commandHandlerFactory) : base(logger)
    {
        _server = server;
        _commandHandlerFactory = commandHandlerFactory;
        _sessionTokenSource = new CancellationTokenSource();
        _eventHandler = (_, request) => OrchestrateAsync(request).Forget();
    }
    
    protected override void OnStartService()
    {
        _server.RequestReceived +=  _eventHandler;
    }

    protected override void OnStopService()
    {
        _server.RequestReceived -= _eventHandler;

        if (!_sessionTokenSource.IsCancellationRequested)
            _sessionTokenSource.Cancel();

        _sessionTokenSource.Dispose();
    }

    /// <summary>
    /// Orchestrates the given request to execute a command and sends a
    /// response to the connected client.
    /// </summary>
    /// <param name="request">Request to orchestrate.</param>
    private async Task OrchestrateAsync(Request request)
    {
        try
        {
            ICommandHandler commandHandler = _commandHandlerFactory.CreateHandler(request.CommandType);
            OperationResult operationResult = await commandHandler.HandleCommandAsync(request, _sessionTokenSource.Token);
            await SendResponseAsync(operationResult);
        }
        catch (Exception ex)
        {
            await SendExceptionAsync(ex);
        }
    }
    
    /// <summary>
    /// Sends a response to the client using the server.
    /// </summary>
    /// <param name="operationResult">Operation result to send to the client.</param>
    private async Task SendResponseAsync(OperationResult operationResult)
    {
        try
        {
            Response response = operationResult.Success 
                ? Response.Ok(operationResult.Message) 
                : Response.BadRequest(operationResult.Message);
            await _server.SendResponseAsync(response);
        }
        catch (Exception ex)
        {
            await SendExceptionAsync(ex);
        }
    }

    /// <summary>
    /// Logs an exception and sends an internal error response to the client.
    /// </summary>
    /// <param name="exception">Exception to log.</param>
    private async Task SendExceptionAsync(Exception exception)
    {
        Logger.LogCritical(exception, "Exception captured at {Time}", DateTimeOffset.Now);
        Response response = Response.InternalError("An internal error occurred.");
        await _server.SendResponseAsync(response);
    }
}
