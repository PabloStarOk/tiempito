using Tiempito.IPC.NET.Messages;

namespace Tiempitod.NET.Commands;

/// <summary>
/// Defines a handler of a certain type of command request.
/// </summary>
public interface ICommandHandler
{
    /// <summary>
    /// Handles a command request.
    /// </summary>
    /// <param name="request">Request to execute a command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An <see cref="OperationResult"/> representing the result of the executed command.</returns>
    public Task<OperationResult> HandleCommandAsync(Request request, CancellationToken cancellationToken = default);
}
