using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Config.Sessions;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models.Commands;
using Tiempito.IPC.Models.Commands.Config;

namespace Tiempito.Daemon.Application.Commands.Config;

/// <summary>
/// Handles the creation of a new session configuration.
/// </summary>
/// <param name="sessionConfigService">Service for managing session configurations.</param>
/// <param name="timeSpanConverter">Service for converting time span strings.</param>
internal sealed class CreateSessionConfigCommandHandler(
    ISessionConfigService sessionConfigService,
    ITimeSpanConverter timeSpanConverter)
    : ICommandHandler
{
    /// <inheritdoc/>
    public bool CanHandle(Command command) => command is CreateSessionConfigCommand;

    /// <inheritdoc/>
    public async ValueTask<OperationResult> HandleAsync(Command command, CancellationToken cancellationToken = default)
    {
        if (command is not CreateSessionConfigCommand createCmd)
        {
            throw new ArgumentException($"Command must be a {nameof(CreateSessionConfigCommand)}.", nameof(command));
        }

        _ = timeSpanConverter.TryConvert(createCmd.DelayBetweenTimes, out TimeSpan delayBetweenTimes);
        if (!timeSpanConverter.TryConvert(createCmd.FocusDuration, out TimeSpan focusDuration))
        {
            return new OperationResult(Success: false, Message: "Focus duration time is not recognized.");
        }

        if (!timeSpanConverter.TryConvert(createCmd.BreakDuration, out TimeSpan breakDuration))
        {
            return new OperationResult(Success: false, Message: "Break duration time is not recognized.");
        }

        OperationResult operationResult = await sessionConfigService.AddConfigAsync(
            new SessionConfig(
                createCmd.SessionConfigId,
                (int)createCmd.TargetCycles,
                delayBetweenTimes,
                focusDuration,
                breakDuration));

        return operationResult;
    }
}
