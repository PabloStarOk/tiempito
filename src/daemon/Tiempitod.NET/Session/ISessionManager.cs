namespace Tiempitod.NET.Session;

/// <summary>
/// Defines a manager of sessions.
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Starts a session of focus and break times.
    /// </summary>
    /// <param name="sessionId">ID of the session to start.</param>
    /// <returns>An <see cref="OperationResult"/> to know if the session was started successfully.</returns>
    public OperationResult StartSession(string sessionId = "", string sessionConfigId = "");

    /// <summary>
    /// Pauses a session that is currently executing.
    /// </summary>
    /// <param name="sessionId">ID of the session to pause.</param>
    /// <returns>An <see cref="OperationResult"/> to know if the session was paused successfully.</returns>
    public OperationResult PauseSession(string sessionId = "");
    
    /// <summary>
    /// Continues a session that is currently paused.
    /// </summary>
    /// <param name="sessionId">ID of the session to resume.</param>
    /// <returns>An <see cref="OperationResult"/> to know if the session was resumed successfully.</returns>
    public OperationResult ResumeSession(string sessionId = "");

    /// <summary>
    /// Cancels the current session.
    /// </summary>
    /// <param name="sessionId">ID of the session to cancel.</param>
    /// <returns>An <see cref="OperationResult"/> to know if the session was cancelled successfully.</returns>
    public OperationResult CancelSession(string sessionId = "");
}
