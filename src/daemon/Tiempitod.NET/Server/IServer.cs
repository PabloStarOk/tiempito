using AsyncEvent;

namespace Tiempitod.NET.Server;

/// <summary>
/// Defines a server to receive requests and send responses.
/// </summary>
public interface IServer
{
    /// <summary>
    /// Event raised when server execution fails due to a critical error.
    /// </summary>
    public event AsyncEventHandler? OnFailed;
    
    /// <summary>
    /// Starts the server.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StartAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Stops the server.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StopAsync();
}
