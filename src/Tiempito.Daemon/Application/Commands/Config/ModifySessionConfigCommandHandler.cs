using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Config.Sessions;
using Tiempito.Daemon.Domain.Commands.Enums;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models;

namespace Tiempito.Daemon.Application.Commands.Config;

/// <summary>
/// Handles modification of session configuration.
/// </summary>
/// <param name="sessionConfigService">Service for managing session configurations.</param>
/// <param name="timeSpanConverter">Service for converting time span strings.</param>
internal sealed class ModifySessionConfigCommandHandler(
    ISessionConfigService sessionConfigService,
    ITimeSpanConverter timeSpanConverter)
    : ICommandHandler
{
    /// <inheritdoc/>
    public bool CanHandle(Command command) =>
        command.CommandType.Equals(nameof(CommandType.Config), StringComparison.OrdinalIgnoreCase)
        && command.SubcommandType == "modify-session";

    /// <inheritdoc/>
    public async ValueTask<OperationResult> HandleAsync(Command command, CancellationToken cancellationToken = default)
    {
        if (!command.Arguments.TryGetValue("session-config-id", out string? sessionId))
        {
            return new OperationResult(Success: false, Message: "Session id was not provided.");
        }

        int? targetCycles = null;
        TimeSpan? delayBetweenTimes = null;
        TimeSpan? focusDuration = null;
        TimeSpan? breakDuration = null;

        if (command.Arguments.TryGetValue("target-cycles", out string? targetCyclesString)
            && int.TryParse(targetCyclesString, out int parsedTargetCycles))
        {
            targetCycles = parsedTargetCycles;
        }

        if (command.Arguments.TryGetValue("delay-times", out string? delayTimesString)
            && timeSpanConverter.TryConvert(delayTimesString, out TimeSpan parsedDelayBetweenTimes))
        {
            delayBetweenTimes = parsedDelayBetweenTimes;
        }

        if (command.Arguments.TryGetValue("focus-duration", out string? focusDurationString)
            && timeSpanConverter.TryConvert(focusDurationString, out TimeSpan parsedFocusDuration))
        {
            focusDuration = parsedFocusDuration;
        }

        if (command.Arguments.TryGetValue("break-duration", out string? breakDurationString)
            && timeSpanConverter.TryConvert(breakDurationString, out TimeSpan parsedBreakDuration))
        {
            breakDuration = parsedBreakDuration;
        }

        return await sessionConfigService.ModifyConfigAsync(
            sessionId,
            targetCycles,
            delayBetweenTimes,
            focusDuration,
            breakDuration);
    }
}
