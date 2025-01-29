using Tiempitod.NET.Common;
using Tiempitod.NET.Configuration.Session;

namespace Tiempitod.NET.Commands.Configuration;

/// <summary>
/// Represents the command to modify an existing session configuration.
/// </summary>
/// <param name="sessionConfigProvider">Provider of session configurations.</param>
/// <param name="arguments">Arguments of the command.</param>
public readonly struct ModifySessionConfigCommand(
    ISessionConfigProvider sessionConfigProvider,
    IReadOnlyDictionary<string, string> arguments)
    : ICommand
{
    public Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement command request parser to create a CommandRequest model to encapsulate validation logic.
        if (!arguments.TryGetValue("session-config-id", out string? sessionId))
            return Task.FromResult(new OperationResult(Success: false, Message: "Session id was not provided."));
        
        if (!sessionConfigProvider.SessionConfigs.TryGetValue(sessionId.ToLower(), out SessionConfig currentSessionConfig))
            return Task.FromResult(new OperationResult(Success: false, Message: $"Session with id '{sessionId}' doesn't exist."));

        if (!arguments.TryGetValue("target-cycles", out string? targetCyclesString)
            || !int.TryParse(targetCyclesString, out int targetCycles))
            targetCycles = currentSessionConfig.TargetCycles;
        
        if (!arguments.TryGetValue("delay-times", out string? delayTimesString)
            || !TryParseDuration(delayTimesString, out TimeSpan delayBetweenTimes))
            delayBetweenTimes = currentSessionConfig.DelayBetweenTimes;
        
        if (!arguments.TryGetValue("focus-duration", out string? focusDurationString)
            || !TryParseDuration(focusDurationString, out TimeSpan focusDuration))
            focusDuration = currentSessionConfig.FocusDuration;
        
        if (!arguments.TryGetValue("break-duration", out string? breakDurationString) 
            || !TryParseDuration(breakDurationString, out TimeSpan breakDuration))
            breakDuration = currentSessionConfig.BreakDuration;
        
        OperationResult operationResult = sessionConfigProvider.SaveSessionConfig
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
        
        return Task.FromResult(operationResult);
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
