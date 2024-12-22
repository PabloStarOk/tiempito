namespace Tiempitod.NET.Session;

public interface ISessionManager
{
    /// <summary>
    /// Starts a session of focus and break times.
    /// </summary>
    /// <param name="stoppingToken">Token to stop the tasks.</param>
    /// <returns>A task representing the async method.</returns>
    public Task StartSessionAsync(CancellationToken stoppingToken);
}
