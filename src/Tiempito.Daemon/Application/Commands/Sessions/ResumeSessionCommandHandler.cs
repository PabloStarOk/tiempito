using Tiempito.Daemon.Application.Sessions;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models.Commands;
using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.Daemon.Application.Commands.Sessions;

/// <summary>
/// Handles the command to resume a session using the provided <see cref="ISessionService"/>.
/// </summary>
internal sealed class ResumeSessionCommandHandler(ISessionService sessionService)
    : ICommandHandler
{
    /// <inheritdoc/>
    public bool CanHandle(Command command) => command is ResumeSessionCommand;

    /// <inheritdoc/>
    public ValueTask<OperationResult> HandleAsync(Command command, CancellationToken cancellationToken = default)
    {
        if (command is not ResumeSessionCommand resumeCmd)
        {
            throw new ArgumentException($"Command must be a {nameof(ResumeSessionCommand)}.", nameof(command));
        }

        var result = sessionService.ResumeSession(resumeCmd.SessionId);
        return ValueTask.FromResult(result);
    }
}