namespace Tiempitod.NET.Configuration.Session;

/// <summary>
/// Represents the keywords to identify in the session configuration files.
/// </summary>
public enum SessionConfigKeyword
{
    /// <summary>
    /// The target number of cycles.
    /// </summary>
    TargetCycles,

    /// <summary>
    /// Delay to start the next time after the last one has been completed.
    /// </summary>
    DelayBetweenTimes,
    
    /// <summary>
    /// The duration of focus periods.
    /// </summary>
    FocusDuration,

    /// <summary>
    /// The duration of break periods.
    /// </summary>
    BreakDuration
}
