namespace Tiempito.Daemon.Domain.Sessions.Enums;

/// <summary>
/// Represents the type of interval in a session.
/// </summary>
public enum SessionIntervalType
{
    /// <summary>
    /// Interval for focused work.
    /// </summary>
    Focus,

    /// <summary>
    /// Interval for taking a break.
    /// </summary>
    Break,

    /// <summary>
    /// Interval for a delay between a focus and a break interval.
    /// </summary>
    Delay,
}
