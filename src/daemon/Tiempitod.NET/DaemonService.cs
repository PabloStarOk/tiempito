namespace Tiempitod.NET;

/// <summary>
/// Defines a top level service used required by the daemon.
/// </summary>
public abstract class DaemonService(ILogger<DaemonService> logger)
{
    protected readonly ILogger<DaemonService> Logger = logger;

    /// <summary>
    /// Starts the service and catch any exception raised.
    /// </summary>
    /// <returns>True if the service was started successfully, false otherwise.</returns>
    public bool StartService()
    {
        try
        {
            OnStartService();
        }
        catch (Exception ex)
        {
            Logger.LogCritical("Exception occurred while starting {service} at {time}, Error: {error}", this, DateTimeOffset.Now, ex.Message);
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Stops the service and catch any exception raised.
    /// </summary>
    /// <returns>True if the service was stopped successfully, false otherwise.</returns>
    public bool StopService()
    {
        try
        {
            OnStopService();
        }
        catch (Exception ex)
        {
            Logger.LogCritical("Exception occurred while stopping {service} at {time}, Error: {error}", this, DateTimeOffset.Now, ex.Message);
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// Executed after daemon is started.
    /// Empty body if some services doesn't need initialize anything.
    /// </summary>
    protected virtual void OnStartService() { }

    /// <summary>
    /// Executed before daemon is stopped.
    /// Empty body if services doesn't need to clean up anything.
    /// </summary>
    protected virtual void OnStopService() { }
}
