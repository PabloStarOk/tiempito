using Tiempito.Daemon.Commands;
using Tiempito.Daemon.Common;
using Tiempito.Daemon.Server.Interfaces;
using Tiempito.IPC.Models;

namespace Tiempito.Daemon.Server;

/// <summary>
/// Handles incoming requests.
/// </summary>
public class CommandHandler : ICommandHandler
{
    private readonly ILogger<CommandHandler> _logger;
    private readonly IEnumerable<CommandCreator> _commandCreators;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="commandCreators">A collection of command creators.</param>
    public CommandHandler(ILogger<CommandHandler> logger, IEnumerable<CommandCreator> commandCreators)
    {
        _logger = logger;
        _commandCreators = commandCreators;
    }
    
    public async Task<Response> HandleAsync(Command command, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse(command.CommandType, ignoreCase: true, out CommandType commandType))
        {
            _logger.LogCritical("Couldn't cast command type \"{CommandType}\"", command.CommandType);
            return Response.BadRequest(command.CorrelationId, $"Couldn't cast command \"{command.CommandType}\"");
        }
        
        if (!_commandCreators.Any())
            throw new InvalidOperationException("Command creators collection is empty.");

        CommandCreator? commandCreator = 
            _commandCreators.FirstOrDefault(c => c.CommandType == commandType);

        if (commandCreator == null)
            throw new InvalidOperationException($"Command creator was null \"{command.CommandType}\"");
        
        ICommand internalCommand = commandCreator.Create(command.SubcommandType, command.Arguments);
        
        OperationResult operationResult = await internalCommand.ExecuteAsync(cancellationToken);
        
        return operationResult.Success
            ? Response.Ok(command.CorrelationId, operationResult.Message)
            : Response.BadRequest(command.CorrelationId, operationResult.Message);
    }
}