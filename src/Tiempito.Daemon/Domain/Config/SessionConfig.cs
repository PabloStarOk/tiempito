namespace Tiempito.Daemon.Domain.Config;

/// <summary>
/// Represents the configuration for a session.
/// </summary>
/// <param name="Id">Unique identifier for the configuration.</param>
/// <param name="TargetCycles">Number of cycles to target in the session.</param>
/// <param name="DelayBetweenTimes">Delay between each cycle.</param>
/// <param name="FocusDuration">Duration of the focus period.</param>
/// <param name="BreakDuration">Duration of the break period.</param>
public sealed record SessionConfig(
    string Id,
    int TargetCycles,
    TimeSpan DelayBetweenTimes,
    TimeSpan FocusDuration,
    TimeSpan BreakDuration);
