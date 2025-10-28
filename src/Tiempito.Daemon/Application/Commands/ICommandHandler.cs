using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models;

namespace Tiempito.Daemon.Application.Commands;

/// <summary>
/// Defines a handler for processing commands.
/// </summary>
public interface ICommandHandler
{
    /// <summary>
    /// Determines whether this handler can process the specified command.
    /// </summary>
    /// <param name="command">The command to check.</param>
    /// <returns><c>true</c> if the handler can process the command; otherwise, <c>false</c>.</returns>
    public bool CanHandle(Command command);

    /// <summary>
    /// Asynchronously handles the specified command.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An <see cref="OperationResult"/> representing the outcome of the operation.</returns>
    public ValueTask<OperationResult> HandleAsync(Command command, CancellationToken cancellationToken = default);
}