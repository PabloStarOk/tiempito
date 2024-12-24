namespace Tiempitod.NET.Session;

/// <summary>
/// Represents a session.
/// </summary>
/// <param name="timeType">Initial time type (focus or break)</param>
/// <param name="elapsed">Initial time elapsed.</param>
/// <param name="focusDuration">Duration of the focus times.</param>
/// <param name="breakDuration">Duration of the break times.</param>
/// <param name="targetCycles">Number of cycles to complete.</param>
public struct Session(TimeType timeType, TimeSpan elapsed, TimeSpan focusDuration, TimeSpan breakDuration, int targetCycles)
{
    // Immutable data.
    public int TargetCycles { get; } = targetCycles;
    public TimeSpan FocusDuration { get; } = focusDuration;
    public TimeSpan BreakDuration { get; } = breakDuration;
    
    // Dynamic data.
    public SessionStatus Status { get; set; }
    public int CurrentCycle { get; set; }
    public TimeType CurrentTimeType { get; set; } = timeType;
    public TimeSpan Elapsed { get; set; } = elapsed;
}
