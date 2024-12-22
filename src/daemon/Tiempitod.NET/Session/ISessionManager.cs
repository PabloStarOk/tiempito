namespace Tiempitod.NET.Session;

public interface ISessionManager
{
    /// <summary>
    /// Starts a session of focus and break times.
    /// </summary>
    /// <param name="stoppingToken">Token to stop the operation.</param>
    /// <returns>A task representing the async operation.</returns>
    public Task StartSessionAsync(CancellationToken stoppingToken);

    /// <summary>
    /// Pauses a session that is currently executing.
    /// </summary>
    public void PauseSession();
    
    /// <summary>
    /// Continues a session that is currently paused.
    /// </summary>
    public void ContinueSession();
}
