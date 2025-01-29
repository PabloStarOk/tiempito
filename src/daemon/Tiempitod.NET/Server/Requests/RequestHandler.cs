using Tiempito.IPC.NET.Messages;

using Tiempitod.NET.Commands;
using Tiempitod.NET.Common;

namespace Tiempitod.NET.Server.Requests;

/// <summary>
/// Handles incoming requests.
/// </summary>
public class RequestHandler : IRequestHandler
{
    private readonly ILogger<RequestHandler> _logger;
    private readonly IEnumerable<CommandCreator> _commandCreators;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="commandCreators">A collection of command creators.</param>
    public RequestHandler(ILogger<RequestHandler> logger, IEnumerable<CommandCreator> commandCreators)
    {
        _logger = logger;
        _commandCreators = commandCreators;
    }
    
    public async Task<Response> HandleAsync(Request request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse(request.CommandType, ignoreCase: true, out CommandType commandType))
        {
            _logger.LogCritical("Couldn't cast command type \"{CommandType}\"", request.CommandType);
            return Response.BadRequest($"Couldn't cast command \"{request.CommandType}\"");
        }
        
        if (!_commandCreators.Any())
            throw new InvalidOperationException("Command creators collection is empty.");

        CommandCreator? commandCreator = 
            _commandCreators.FirstOrDefault(c => c.CommandType == commandType);

        if (commandCreator == null)
            throw new InvalidOperationException($"Command creator was null \"{request.CommandType}\"");
        
        ICommand command = commandCreator.Create(request.SubcommandType, request.Arguments);
        
        OperationResult operationResult = await command.ExecuteAsync(cancellationToken);
        
        return operationResult.Success 
            ? Response.Ok(operationResult.Message)
            : Response.BadRequest(operationResult.Message);
    }
}