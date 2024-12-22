namespace Tiempitod.NET.Session;

public struct Session(TimeType timeType, TimeSpan elapsed, TimeSpan duration, int currentCycle = 0)
{
    public TimeType CurrentTimeType { get; set; } = timeType;
    public TimeSpan Elapsed { get; set; } = elapsed;
    public TimeSpan Duration { get; set; } = duration;
    public int CurrentCycle { get; set; } = currentCycle;
    public SessionStatus Status { get; set; }
}
