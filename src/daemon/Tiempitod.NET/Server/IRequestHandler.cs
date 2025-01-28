using Tiempito.IPC.NET.Messages;

namespace Tiempitod.NET.Server;

/// <summary>
/// Defines a handler of requests.
/// </summary>
public interface IRequestHandler
{
    /// <summary>
    /// Handles a request.
    /// </summary>
    /// <param name="request">Request to be handled.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="Response"/>.</returns>
    public Task<Response> HandleAsync(Request request, CancellationToken cancellationToken);
}