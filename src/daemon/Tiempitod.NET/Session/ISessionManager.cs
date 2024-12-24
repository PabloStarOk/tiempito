namespace Tiempitod.NET.Session;

/// <summary>
/// Defines a manager of sessions.
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Starts a session of focus and break times.
    /// </summary>
    /// <param name="stoppingToken">Token to stop the operation.</param>
    /// <returns>An <see cref="OperationResult"/> to know if the session was started successfully.</returns>
    public OperationResult StartSession(CancellationToken stoppingToken);

    /// <summary>
    /// Pauses a session that is currently executing.
    /// </summary>
    /// <returns>An <see cref="OperationResult"/> to know if the session was paused successfully.</returns>
    public Task<OperationResult> PauseSessionAsync();
    
    /// <summary>
    /// Continues a session that is currently paused.
    /// </summary>
    /// <returns>An <see cref="OperationResult"/> to know if the session was resumed successfully.</returns>
    public OperationResult ResumeSession();

    /// <summary>
    /// Cancels the current session.
    /// </summary>
    /// <returns>An <see cref="OperationResult"/> to know if the session was cancelled successfully.</returns>
    public Task<OperationResult> CancelSessionAsync();
}
