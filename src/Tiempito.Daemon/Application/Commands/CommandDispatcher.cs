using Tiempito.IPC.Models;
using Tiempito.IPC.Models.Commands;

namespace Tiempito.Daemon.Application.Commands;

/// <summary>
/// Dispatches commands to their appropriate handlers within a scoped service context.
/// </summary>
internal sealed class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandDispatcher"/> class.
    /// </summary>
    /// <param name="scopeFactory">The factory used to create service scopes for command handling.</param>
    public CommandDispatcher(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <inheritdoc/>
    public async ValueTask<Response> DispatchAsync(Command command, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var commandHandlers = scope.ServiceProvider.GetRequiredService<IEnumerable<ICommandHandler>>();
        var handler = commandHandlers.First(c => c.CanHandle(command));
        var result = await handler.HandleAsync(command, cancellationToken);
        return result.Success
            ? Response.Ok(command.CorrelationId, result.Message)
            : Response.BadRequest(command.CorrelationId, result.Message);
    }
}