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
    /// <returns>A task representing the async operation.</returns>
    public void StartSession(CancellationToken stoppingToken);

    /// <summary>
    /// Pauses a session that is currently executing.
    /// </summary>
    public Task PauseSessionAsync();
    
    /// <summary>
    /// Continues a session that is currently paused.
    /// </summary>
    public void ResumeSession();

    /// <summary>
    /// Cancels the current session.
    /// </summary>
    public Task CancelSessionAsync();
}
