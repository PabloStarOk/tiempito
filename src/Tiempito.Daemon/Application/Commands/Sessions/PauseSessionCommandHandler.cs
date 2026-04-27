using Tiempito.Daemon.Application.Sessions;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models.Commands;
using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.Daemon.Application.Commands.Sessions;

/// <summary>
/// Handles the pause session command by delegating to the <see cref="ISessionService"/>.
/// </summary>
/// <param name="sessionService">Service for session operations.</param>
internal sealed class PauseSessionCommandHandler(ISessionService sessionService)
    : ICommandHandler
{
    /// <inheritdoc/>
    public bool CanHandle(Command command) => command is PauseSessionCommand;

    /// <inheritdoc/>
    public ValueTask<OperationResult> HandleAsync(Command command, CancellationToken cancellationToken = default)
    {
        if (command is not PauseSessionCommand pauseCmd)
        {
            throw new ArgumentException($"Command must be a {nameof(PauseSessionCommand)}.", nameof(command));
        }

        var result = sessionService.PauseSession(pauseCmd.SessionId);
        return ValueTask.FromResult(result);
    }
}
