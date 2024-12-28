namespace Tiempitod.NET.Configuration.Session;

public struct SessionConfig
{
    public string Id { get; }
    public int TargetCycles { get;  }
    public TimeSpan FocusDuration { get; }
    public TimeSpan BreakDuration { get; }

    public SessionConfig()
    {
        Id = "Default";
        TargetCycles = 4;
        FocusDuration = TimeSpan.FromMinutes(25);
        BreakDuration = TimeSpan.FromMinutes(5);
    }
    
    public SessionConfig(string id, int targetCycles, TimeSpan focusDuration, TimeSpan breakDuration, bool isDefault = false)
    {
        Id = id;
        TargetCycles = targetCycles;
        FocusDuration = focusDuration;
        BreakDuration = breakDuration;
    }
}
