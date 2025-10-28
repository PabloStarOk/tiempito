using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Config.Sessions;
using Tiempito.Daemon.Domain.Commands.Enums;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models;

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
    public bool CanHandle(Command command) =>
        command.CommandType.Equals(nameof(CommandType.Config), StringComparison.OrdinalIgnoreCase)
        && command.SubcommandType == "create-session";

    /// <inheritdoc/>
    public async ValueTask<OperationResult> HandleAsync(Command command, CancellationToken cancellationToken = default)
    {
        if (!command.Arguments.TryGetValue("session-config-id", out string? sessionId))
        {
            return new OperationResult(Success: false, Message: "Session id was not provided.");
        }

        if (!command.Arguments.TryGetValue("target-cycles", out string? targetCyclesString))
        {
            return new OperationResult(Success: false, Message: "Target cycles was not provided.");
        }

        if (!int.TryParse(targetCyclesString, out int targetCycles))
        {
            return new OperationResult(Success: false, Message: "Target cycles number provided is not recognized.");
        }

        TimeSpan delayBetweenTimes = TimeSpan.Zero;
        if (command.Arguments.TryGetValue("delay-times", out string? delayTimesString))
        {
            timeSpanConverter.TryConvert(delayTimesString, out delayBetweenTimes);
        }

        if (!command.Arguments.TryGetValue("focus-duration", out string? focusDurationString))
        {
            return new OperationResult(Success: false, Message: "Focus duration was not provided.");
        }

        if (!timeSpanConverter.TryConvert(focusDurationString, out TimeSpan focusDuration))
        {
            return new OperationResult(Success: false, Message: "Focus duration time is not recognized.");
        }

        if (!command.Arguments.TryGetValue("break-duration", out string? breakDurationString))
        {
            return new OperationResult(Success: false, Message: "Focus duration was not provided.");
        }

        if (!timeSpanConverter.TryConvert(breakDurationString, out TimeSpan breakDuration))
        {
            return new OperationResult(Success: false, Message: "Focus duration time is not recognized.");
        }

        OperationResult operationResult = await sessionConfigService.AddConfigAsync(
            new SessionConfig(
                sessionId,
                targetCycles,
                delayBetweenTimes,
                focusDuration,
                breakDuration));

        return operationResult;
    }
}
