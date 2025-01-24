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
            Logger.LogCritical(ex, "Exception occurred while starting {Service} daemon service at {Time}", this, DateTimeOffset.Now);
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
            Logger.LogCritical(ex, "Exception occurred while stopping {Service} at {Time}", this, DateTimeOffset.Now);
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

    /// <summary>
    /// Regenerates a cancellation token source managed by the daemon service.
    /// </summary>
    /// <param name="oldTokenSource">Token source to regenerate.</param>
    protected static CancellationTokenSource RegenerateTokenSource(CancellationTokenSource oldTokenSource)
    {
        try
        {
            if (oldTokenSource.TryReset())
                return oldTokenSource;
        }
        catch (ObjectDisposedException)
        {
            return new CancellationTokenSource();
        }
        
        oldTokenSource.Dispose();
        return new CancellationTokenSource();
    }
}
