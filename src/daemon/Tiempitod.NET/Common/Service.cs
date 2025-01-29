namespace Tiempitod.NET.Common;

/// <summary>
/// Defines a top level service required in the Daemon.
/// </summary>
public abstract class Service(ILogger<Service> logger)
{
    protected readonly ILogger<Service> _logger = logger;

    /// <summary>
    /// Starts the service and catch any exception raised.
    /// </summary>
    /// <returns>True if the service was started successfully, false otherwise.</returns>
    public async Task<bool> StartServiceAsync()
    {
        try
        {
            return await OnStartServiceAsync();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Exception occurred while starting {Service} daemon service at {Time}", this, DateTimeOffset.Now);
            return false;
        }
    }
    
    /// <summary>
    /// Stops the service and catch any exception raised.
    /// </summary>
    /// <returns>True if the service was stopped successfully, false otherwise.</returns>
    public async Task<bool> StopServiceAsync()
    {
        try
        {
            return await OnStopServiceAsync();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Exception occurred while stopping {Service} at {Time}", this, DateTimeOffset.Now);
            return false;
        }
    }

    /// <summary>
    /// Executed after daemon is started.
    /// Empty body if some services doesn't need initialize anything.
    /// </summary>
    protected abstract Task<bool> OnStartServiceAsync();

    /// <summary>
    /// Executed before daemon is stopped.
    /// Empty body if services doesn't need to clean up anything.
    /// </summary>
    protected abstract Task<bool> OnStopServiceAsync();

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
