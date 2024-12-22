namespace Tiempitod.NET.Session;

public struct SessionProgress(TimeType timeType, TimeSpan elapsed, TimeSpan duration, int currentCycle = 0)
{
    public TimeType TimeType { get; set; } = timeType;
    public TimeSpan Elapsed { get; set; } = elapsed;
    public TimeSpan Duration { get; set; } = duration;
    public int CurrentCycle { get; set; } = currentCycle;
    public SessionStatus Status { get; set; }
}
