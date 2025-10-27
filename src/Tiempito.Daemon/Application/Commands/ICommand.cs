using Tiempito.Daemon.Domain.Shared;

namespace Tiempito.Daemon.Application.Commands;

/// <summary>
/// Defines a command.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    public Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default);
}
