using Tiempito.IPC.Models;

namespace Tiempito.Daemon.Server.Interfaces;

/// <summary>
/// Defines a handler of requests.
/// </summary>
public interface ICommandHandler
{
    /// <summary>
    /// Handles a request.
    /// </summary>
    /// <param name="command">Request to be handled.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="Response"/>.</returns>
    public Task<Response> HandleAsync(Command command, CancellationToken cancellationToken);
}