using Tiempitod.NET.Common;

namespace Tiempitod.NET.Commands;

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
