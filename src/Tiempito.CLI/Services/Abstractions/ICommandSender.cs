using Tiempito.IPC.Models;
using Tiempito.IPC.Models.Commands;

namespace Tiempito.CLI.Services.Abstractions;

/// <summary>
/// Defines a service capable of sending commands.
/// </summary>
public interface ICommandSender
{
    /// <summary>
    /// Sends a command asynchronously.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to a <see cref="Response"/> instance, or <c>null</c>
    /// if no response is available.
    /// </returns>
    public Task<Response> SendAsync(Command command, CancellationToken cancellationToken = default);
}