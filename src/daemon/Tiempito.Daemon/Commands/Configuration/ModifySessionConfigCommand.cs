using Tiempito.Daemon.Common;
using Tiempito.Daemon.Configuration.Session.Enums;
using Tiempito.Daemon.Configuration.Session.Interfaces;

namespace Tiempito.Daemon.Commands.Configuration;

/// <summary>
/// Represents the command to modify an existing session configuration.
/// </summary>
/// <param name="sessionConfigService">Service to modify session configurations.</param>
/// <param name="arguments">Arguments of the command.</param>
public readonly struct ModifySessionConfigCommand(
    ISessionConfigService sessionConfigService,
    IReadOnlyDictionary<string, string> arguments)
    : ICommand
{
    public async Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement command request parser to create a CommandRequest model to encapsulate validation logic.
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
            && TryParseDuration(delayTimesString, out TimeSpan parsedDelayBetweenTimes))
            delayBetweenTimes = parsedDelayBetweenTimes;
        
        if (arguments.TryGetValue("focus-duration", out string? focusDurationString)
            && TryParseDuration(focusDurationString, out TimeSpan parsedFocusDuration))
            focusDuration = parsedFocusDuration;
        
        if (arguments.TryGetValue("break-duration", out string? breakDurationString) 
            && TryParseDuration(breakDurationString, out TimeSpan parsedBreakDuration))
            breakDuration = parsedBreakDuration;
        
        return await sessionConfigService.ModifyConfigAsync(
            sessionId,
            targetCycles,
            delayBetweenTimes,
            focusDuration,
            breakDuration);
    }
    
    // TODO: Create time span IFormatProvider to parse the configuration.
    /// <summary>
    /// Maps enums <see cref="SessionDurationSymbol"/> to a string representing that unit in lower case.
    /// </summary>
    private readonly static Dictionary<SessionDurationSymbol, string> TimeUnitSymbolsMap = new()
    {
        { SessionDurationSymbol.Millisecond, "ms" },
        { SessionDurationSymbol.Second, "s" },
        { SessionDurationSymbol.Minute, "m" },
        { SessionDurationSymbol.Hour, "h" }
    };
    
    /// <summary>
    /// Tries to extract a time unit symbol from the given string and parses the value.
    /// </summary>
    /// <param name="timeString">String to extract and parse.</param>
    /// <param name="durationSymbol">Time unit symbol to try to extract.</param>
    /// <param name="parsedTime">String parsed to an integer value.</param>
    /// <returns>True if the given duration symbol is correct and the time was parsed successfully, false otherwise.</returns>
    private static bool TryExtractTimeUnit(string timeString, SessionDurationSymbol durationSymbol, out int parsedTime)
    {
        parsedTime = 0;

        string timeStrLowerCase = timeString.ToLower();
        string symbolString = TimeUnitSymbolsMap[durationSymbol].ToLower();

        if (!timeStrLowerCase.EndsWith(symbolString))
            return false;
        
        string amountString = timeStrLowerCase.Replace(symbolString, "");
        return int.TryParse(amountString, out parsedTime);
    }

    /// <summary>
    /// Parse a string as a TimeSpan duration.
    /// </summary>
    /// <param name="timeString">String to parse.</param>
    /// <param name="duration">Parsed value.</param>
    /// <returns>True if the string was parsed successfully, false otherwise.</returns>
    private static bool TryParseDuration(string? timeString, out TimeSpan duration)
    {
        duration = TimeSpan.Zero;

        if (timeString == null)
            return false;
        
        if (timeString == "0")
            return true;
        
        if (TryExtractTimeUnit(timeString, SessionDurationSymbol.Millisecond, out int parsedTime))
        {
            duration = TimeSpan.FromMilliseconds(parsedTime);
            return true;
        }

        if (TryExtractTimeUnit(timeString, SessionDurationSymbol.Second, out parsedTime))
        {
            duration = TimeSpan.FromSeconds(parsedTime);
            return true;
        }

        if (TryExtractTimeUnit(timeString, SessionDurationSymbol.Minute, out parsedTime))
        {
            duration = TimeSpan.FromMinutes(parsedTime);
            return true;
        }

        if (!TryExtractTimeUnit(timeString, SessionDurationSymbol.Hour, out parsedTime))
            return false;

        duration = TimeSpan.FromHours(parsedTime);
        return true;
    }
}
