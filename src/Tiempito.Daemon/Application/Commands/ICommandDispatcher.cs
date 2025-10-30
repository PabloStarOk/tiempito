using Tiempito.IPC.Models;

namespace Tiempito.Daemon.Application.Commands;

/// <summary>
/// Defines a dispatcher for handling <see cref="Command"/> instances asynchronously.
/// </summary>
public interface ICommandDispatcher
{
    /// <summary>
    /// Dispatches the specified <paramref name="command"/> and returns a <see cref="Response"/> asynchronously.
    /// </summary>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask{Response}"/> representing the asynchronous operation.</returns>
    public ValueTask<Response> DispatchAsync(Command command, CancellationToken cancellationToken = default);
}