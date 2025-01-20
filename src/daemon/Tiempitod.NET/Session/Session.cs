namespace Tiempitod.NET.Session;

/// <summary>
/// Represents a session.
/// </summary>
public struct Session
{
    // Immutable data.
    public string Id { get; }
    public int TargetCycles { get; }
    public TimeSpan DelayBetweenTimes { get; }
    public TimeSpan FocusDuration { get; }
    public TimeSpan BreakDuration { get; }
    
    // Dynamic data.
    public SessionStatus Status { get; set; } = SessionStatus.None;
    public int CurrentCycle { get; set; } = 0;
    public TimeType CurrentTimeType { get; set; } = TimeType.Focus;
    public TimeSpan Elapsed { get; set; } = TimeSpan.Zero;
    
    /// <summary>
    /// Instantiates a new <see cref="Session"/>.
    /// </summary>
    /// <param name="id">ID of the session.</param>
    /// <param name="delayBetweenTimes">Delay between times.</param>
    /// <param name="targetCycles">Number of cycles to complete.</param>
    /// <param name="focusDuration">Duration of the focus times.</param>
    /// <param name="breakDuration">Duration of the break times.</param>
    public Session(string id, int targetCycles, TimeSpan delayBetweenTimes, TimeSpan focusDuration, TimeSpan breakDuration)
    {
        Id = id;
        TargetCycles = targetCycles;
        DelayBetweenTimes = delayBetweenTimes;
        FocusDuration = focusDuration;
        BreakDuration = breakDuration;
    }
}
