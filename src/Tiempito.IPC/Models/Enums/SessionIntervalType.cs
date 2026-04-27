namespace Tiempito.IPC.Models.Enums;

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
    /// Interval for a delay between focus and break intervals.
    /// </summary>
    Delay,
}