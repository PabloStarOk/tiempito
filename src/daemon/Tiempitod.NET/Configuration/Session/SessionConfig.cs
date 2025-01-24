namespace Tiempitod.NET.Configuration.Session;

/// <summary>
/// Represents the configuration of a session.
/// </summary>
public struct SessionConfig
{
    public string Id { get; }
    /// <summary>
    /// Number of focus and breaks to complete, 0 means no limit.
    /// </summary>
    public int TargetCycles { get;  }
    
    /// <summary>
    /// Delay to start a time when the last time was completed.
    /// </summary>
    public TimeSpan DelayBetweenTimes { get; }
    public TimeSpan FocusDuration { get; }
    public TimeSpan BreakDuration { get; }

    /// <summary>
    /// Instantiates a new <see cref="SessionConfig"/>
    /// </summary>
    public SessionConfig()
    {
        Id = "Default";
        TargetCycles = 4;
        DelayBetweenTimes = TimeSpan.FromSeconds(10);
        FocusDuration = TimeSpan.FromMinutes(25);
        BreakDuration = TimeSpan.FromMinutes(5);
    }
    
    /// <summary>
    /// Instantiates a new <see cref="SessionConfig"/>
    /// </summary>
    /// <param name="id">ID of the session.</param>
    /// <param name="targetCycles">Target cycles to complete.</param>
    /// <param name="delayBetweenTimes">Delay to start a time when the last time was completed.</param>
    /// <param name="focusDuration">Duration of the focus time.</param>
    /// <param name="breakDuration">Duration of the break time.</param>
    public SessionConfig(string id, int targetCycles,
        TimeSpan delayBetweenTimes, TimeSpan focusDuration, TimeSpan breakDuration)
    {
        Id = id;
        TargetCycles = targetCycles;
        DelayBetweenTimes = delayBetweenTimes;
        FocusDuration = focusDuration;
        BreakDuration = breakDuration;
    }
}
