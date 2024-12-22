namespace Tiempitod.NET.Session;

/// <summary>
/// Represents a session.
/// </summary>
/// <param name="timeType">Initial time type (focus or break)</param>
/// <param name="elapsed">Initial time elapsed.</param>
/// <param name="duration">Duration of the current time type.</param>
/// <param name="currentCycle">Initial number of cycles completed.</param>
public struct Session(TimeType timeType, TimeSpan elapsed, TimeSpan duration, int currentCycle = 0)
{
    public TimeType CurrentTimeType { get; set; } = timeType;
    public TimeSpan Elapsed { get; set; } = elapsed;
    public TimeSpan Duration { get; set; } = duration;
    public int CurrentCycle { get; set; } = currentCycle;
    public SessionStatus Status { get; set; }
}
