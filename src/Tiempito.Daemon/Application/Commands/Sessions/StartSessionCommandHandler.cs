using Tiempito.Daemon.Application.Sessions;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models.Commands;
using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.Daemon.Application.Commands.Sessions;

/// <summary>
/// Handles the start session command by invoking the session service.
/// </summary>
/// <param name="sessionService">Service responsible for session operations.</param>
internal sealed class StartSessionCommandHandler(ISessionService sessionService)
    : ICommandHandler
{
    /// <inheritdoc/>
    public bool CanHandle(Command command) => command is StartSessionCommand;

    /// <inheritdoc/>
    public async ValueTask<OperationResult> HandleAsync(Command command, CancellationToken cancellationToken = default)
    {
        if (command is not StartSessionCommand startCmd)
        {
            throw new ArgumentException($"Command must be a {nameof(StartSessionCommand)}.", nameof(command));
        }

        return await sessionService.StartSessionAsync(startCmd.SessionId, startCmd.SessionConfigId);
    }
}
