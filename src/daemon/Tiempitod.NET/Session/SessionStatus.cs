namespace Tiempitod.NET.Session;

/// <summary>
/// Status of a session.
/// </summary>
public enum SessionStatus
{
    /// <summary>
    /// A session that is executing.
    /// </summary>
    Executing,
    /// <summary>
    /// A session that is paused.
    /// </summary>
    Paused,
    /// <summary>
    /// A session that is cancelled.
    /// </summary>
    Cancelled,
    /// <summary>
    /// A session that is finished.
    /// </summary>
    Finished
}
