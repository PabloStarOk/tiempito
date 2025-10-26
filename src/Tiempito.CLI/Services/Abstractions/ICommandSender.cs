using Tiempito.IPC.Models;

namespace Tiempito.CLI.Services.Abstractions;

/// <summary>
/// Defines a service capable of sending commands.
/// </summary>
public interface ICommandSender
{
    /// <summary>
    /// Sends a command asynchronously.
    /// </summary>
    /// <param name="commandType">The top-level command identifier.</param>
    /// <param name="subcommandType">The subcommand identifier within the command type.</param>
    /// <param name="args">A read-only dictionary containing command arguments (key/value pairs).</param>
    /// <param name="follow">
    /// If <c>true</c>, the CLI app must follow the session progress.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to a <see cref="Response"/> instance, or <c>null</c>
    /// if no response is available.
    /// </returns>
    public Task<Response?> SendAsync(
        string commandType,
        string subcommandType,
        IReadOnlyDictionary<string, string> args,
        bool follow = false,
        CancellationToken cancellationToken = default);
}