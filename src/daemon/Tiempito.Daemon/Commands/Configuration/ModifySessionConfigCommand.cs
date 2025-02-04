using Tiempito.Daemon.Common;
using Tiempito.Daemon.Common.Interfaces;
using Tiempito.Daemon.Configuration.Session.Interfaces;

namespace Tiempito.Daemon.Commands.Configuration;

/// <summary>
/// Represents the command to modify an existing session configuration.
/// </summary>
/// <param name="sessionConfigService">Service to modify session configurations.</param>
/// <param name="timeSpanConverter">Converter of time span.</param>
/// <param name="arguments">Arguments of the command.</param>
public readonly struct ModifySessionConfigCommand(
    ISessionConfigService sessionConfigService,
    ITimeSpanConverter timeSpanConverter,
    IReadOnlyDictionary<string, string> arguments)
    : ICommand
{
    public async Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!arguments.TryGetValue("session-config-id", out string? sessionId))
            return new OperationResult(Success: false, Message: "Session id was not provided.");

        int? targetCycles = null;
        TimeSpan? delayBetweenTimes = null;
        TimeSpan? focusDuration = null;
        TimeSpan? breakDuration = null;
        
        if (arguments.TryGetValue("target-cycles", out string? targetCyclesString)
            && int.TryParse(targetCyclesString, out int parsedTargetCycles))
            targetCycles = parsedTargetCycles;
        
        if (arguments.TryGetValue("delay-times", out string? delayTimesString)
            && timeSpanConverter.TryConvert(delayTimesString, out TimeSpan parsedDelayBetweenTimes))
            delayBetweenTimes = parsedDelayBetweenTimes;
        
        if (arguments.TryGetValue("focus-duration", out string? focusDurationString)
            && timeSpanConverter.TryConvert(focusDurationString, out TimeSpan parsedFocusDuration))
            focusDuration = parsedFocusDuration;
        
        if (arguments.TryGetValue("break-duration", out string? breakDurationString) 
            && timeSpanConverter.TryConvert(breakDurationString, out TimeSpan parsedBreakDuration))
            breakDuration = parsedBreakDuration;
        
        return await sessionConfigService.ModifyConfigAsync(
            sessionId,
            targetCycles,
            delayBetweenTimes,
            focusDuration,
            breakDuration);
    }
}
