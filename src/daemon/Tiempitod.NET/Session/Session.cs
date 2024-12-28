namespace Tiempitod.NET.Session;

/// <summary>
/// Represents a session.
/// </summary>
/// <param name="timeType">Initial time type (focus or break)</param>
/// <param name="elapsed">Initial time elapsed.</param>
/// <param name="focusDuration">Duration of the focus times.</param>
/// <param name="breakDuration">Duration of the break times.</param>
/// <param name="targetCycles">Number of cycles to complete.</param>
public struct Session
{
    // Immutable data.
    public int TargetCycles { get; }
    public TimeSpan FocusDuration { get; }
    public TimeSpan BreakDuration { get; }
    
    // Dynamic data.
    public SessionStatus Status { get; set; } = SessionStatus.None;
    public int CurrentCycle { get; set; } = 0;
    public TimeType CurrentTimeType { get; set; } = TimeType.Focus;
    public TimeSpan Elapsed { get; set; } = TimeSpan.Zero;
    
    public Session(int targetCycles, TimeSpan focusDuration, TimeSpan breakDuration)
    {
        TargetCycles = targetCycles;
        FocusDuration = focusDuration;
        BreakDuration = breakDuration;
    }
}
