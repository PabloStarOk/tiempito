using Tiempito.Daemon.Application.Sessions;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models.Commands;
using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.Daemon.Application.Commands.Sessions;

/// <summary>
/// Handles the cancellation of sessions by invoking the <see cref="ISessionService"/>.
/// </summary>
/// <param name="sessionService">Service used to manage sessions.</param>
internal sealed class CancelSessionCommandHandler(ISessionService sessionService)
    : ICommandHandler
{
    /// <inheritdoc/>
    public bool CanHandle(Command command) => command is CancelSessionCommand;

    /// <inheritdoc/>
    public ValueTask<OperationResult> HandleAsync(Command command, CancellationToken cancellationToken = default)
    {
        if (command is not CancelSessionCommand cancelCmd)
        {
            throw new ArgumentException($"Command must be a {nameof(CancelSessionCommand)}.", nameof(command));
        }

        var result = sessionService.CancelSession(cancelCmd.SessionId);
        return ValueTask.FromResult(result);
    }
}
