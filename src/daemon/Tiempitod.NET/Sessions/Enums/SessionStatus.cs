namespace Tiempitod.NET.Sessions.Enums;

/// <summary>
/// Status of a session.
/// </summary>
public enum SessionStatus
{
    /// <summary>
    /// A session recently initialized.
    /// </summary>
    None,
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
