using Tiempito.Daemon.Common;
using Tiempito.Daemon.Common.Interfaces;
using Tiempito.Daemon.Configuration.Session.Interfaces;
using Tiempito.Daemon.Configuration.Session.Objects;

namespace Tiempito.Daemon.Commands.Configuration;

/// <summary>
/// Represents a command to create a new session configuration.
/// </summary>
/// <param name="sessionConfigService">Service to add the created session configurations.</param>
/// <param name="timeSpanConverter">Converter of time span.</param>
/// <param name="arguments">Arguments of the command.</param>
public readonly struct CreateSessionConfigCommand(
    ISessionConfigService sessionConfigService,
    ITimeSpanConverter timeSpanConverter,
    IReadOnlyDictionary<string, string> arguments)
    : ICommand
{
    public async Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!arguments.TryGetValue("session-config-id", out string? sessionId))
            return new OperationResult(Success: false, Message: "Session id was not provided.");
        
        if (!arguments.TryGetValue("target-cycles", out string? targetCyclesString))
            return new OperationResult(Success: false, Message: "Target cycles was not provided.");
        if (!int.TryParse(targetCyclesString, out int targetCycles))
            return new OperationResult(Success: false, Message: "Target cycles number provided is not recognized.");

        TimeSpan delayBetweenTimes = TimeSpan.Zero;
        if (arguments.TryGetValue("delay-times", out string? delayTimesString))
            timeSpanConverter.TryConvert(delayTimesString, out delayBetweenTimes);
        
        if (!arguments.TryGetValue("focus-duration", out string? focusDurationString))
            return new OperationResult(Success: false, Message: "Focus duration was not provided.");
        if (!timeSpanConverter.TryConvert(focusDurationString, out TimeSpan focusDuration))
            return new OperationResult(Success: false, Message: "Focus duration time is not recognized.");
        
        if (!arguments.TryGetValue("break-duration", out string? breakDurationString))
            return new OperationResult(Success: false, Message: "Focus duration was not provided.");
        if (!timeSpanConverter.TryConvert(breakDurationString, out TimeSpan breakDuration))
            return new OperationResult(Success: false, Message: "Focus duration time is not recognized.");
        
        OperationResult operationResult = await sessionConfigService.AddConfigAsync
        (
            new SessionConfig
            (
                sessionId,
                targetCycles,
                delayBetweenTimes,
                focusDuration,
                breakDuration
            )
        );
        
        return operationResult;
    }
}
