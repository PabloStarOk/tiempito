namespace Tiempitod.NET.Server;

/// <summary>
/// Defines a server to receive and handle commands. 
/// </summary>
public interface ICommandServer
{
    public event EventHandler<string> CommandReceived;
    
    /// <summary>
    /// Starts the server to handle command requests.
    /// </summary>
    public void Start();

    /// <summary>
    /// Restarts the server to handle command requests again.
    /// </summary>
    public void Restart();
    
    /// <summary>
    /// Stops the server asynchronously.
    /// </summary>
    /// <returns>Returns a Task representing the operation.</returns>
    public Task StopAsync();

    /// <summary>
    /// Sends a response to the current connected client.
    /// </summary>
    /// <returns>A Task representing the operation.</returns>
    public Task SendResponseAsync(DaemonResponse response);
}
