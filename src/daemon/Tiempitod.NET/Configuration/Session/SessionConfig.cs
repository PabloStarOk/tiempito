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
    public TimeSpan FocusDuration { get; }
    public TimeSpan BreakDuration { get; }

    /// <summary>
    /// Instantiates a new <see cref="SessionConfig"/>
    /// </summary>
    public SessionConfig()
    {
        Id = "Default";
        TargetCycles = 4;
        FocusDuration = TimeSpan.FromMinutes(25);
        BreakDuration = TimeSpan.FromMinutes(5);
    }
    
    /// <summary>
    /// Instantiates a new <see cref="SessionConfig"/>
    /// </summary>
    /// <param name="id">Id of the session.</param>
    /// <param name="targetCycles">Target cycles to complete.</param>
    /// <param name="focusDuration">Duration of the focus time.</param>
    /// <param name="breakDuration">Duration of the break time.</param>
    public SessionConfig(string id, int targetCycles, TimeSpan focusDuration, TimeSpan breakDuration)
    {
        Id = id;
        TargetCycles = targetCycles;
        FocusDuration = focusDuration;
        BreakDuration = breakDuration;
    }
}
