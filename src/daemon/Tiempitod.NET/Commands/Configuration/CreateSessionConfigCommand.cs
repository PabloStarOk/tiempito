using Tiempitod.NET.Configuration.Session;

namespace Tiempitod.NET.Commands.Configuration;

/// <summary>
/// Represents a command to create a new session configuration.
/// </summary>
/// <param name="sessionConfigProvider">Provider of session configurations.</param>
/// <param name="arguments">Arguments of the command.</param>
public readonly struct CreateSessionConfigCommand(
    ISessionConfigProvider sessionConfigProvider,
    IReadOnlyDictionary<string, string> arguments)
    : ICommand
{
    public Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement command request parser to create a CommandRequest model to encapsulate validation logic.
        if (!arguments.TryGetValue("session-config-id", out string? sessionId))
            return Task.FromResult(new OperationResult(Success: false, Message: "Session id was not provided."));
        
        if (sessionConfigProvider.SessionConfigs.TryGetValue(sessionId.ToLower(), out _))
            return Task.FromResult(new OperationResult(Success: false, Message: $"Session with id '{sessionId}' already exists."));
        
        if (!arguments.TryGetValue("target-cycles", out string? targetCyclesString))
            return Task.FromResult(new OperationResult(Success: false, Message: "Target cycles was not provided."));
        if (!int.TryParse(targetCyclesString, out int targetCycles))
            return Task.FromResult(new OperationResult(Success: false, Message: "Target cycles number provided is not recognized."));
        
        if (!arguments.TryGetValue("delay-times", out string? delayTimesString))
            return Task.FromResult(new OperationResult(Success: false, Message: "Focus duration was not provided."));
        if (!TryParseDuration(delayTimesString, out TimeSpan delayBetweenTimes))
            return Task.FromResult(new OperationResult(Success: false, Message: "Focus duration time is not recognized."));
        
        if (!arguments.TryGetValue("focus-duration", out string? focusDurationString))
            return Task.FromResult(new OperationResult(Success: false, Message: "Focus duration was not provided."));
        if (!TryParseDuration(focusDurationString, out TimeSpan focusDuration))
            return Task.FromResult(new OperationResult(Success: false, Message: "Focus duration time is not recognized."));
        
        if (!arguments.TryGetValue("break-duration", out string? breakDurationString))
            return Task.FromResult(new OperationResult(Success: false, Message: "Focus duration was not provided."));
        if (!TryParseDuration(breakDurationString, out TimeSpan breakDuration))
            return Task.FromResult(new OperationResult(Success: false, Message: "Focus duration time is not recognized."));
        
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
    private readonly static  Dictionary<SessionDurationSymbol, string> TimeUnitSymbolsMap = new()
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
