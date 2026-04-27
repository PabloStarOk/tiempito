namespace Tiempito.Daemon.Domain.Sessions.Abstractions;

/// <summary>
/// Represents a timer that triggers events at specified intervals and supports asynchronous waiting for the next tick.
/// </summary>
public interface IPeriodicTimer : IDisposable
{
    /// <summary>Gets or sets the period between ticks.</summary>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="value" /> must be <see cref="F:System.Threading.Timeout.InfiniteTimeSpan" /> or represent a number of milliseconds equal to or larger than 1 and smaller than <see cref="F:System.UInt32.MaxValue" />.</exception>
    public TimeSpan Period { get; set; }

    /// <summary>Waits for the next tick of the timer, or for the timer to be stopped.</summary>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> for cancelling the asynchronous wait. If cancellation is requested, it affects only the single wait operation; the underlying timer continues firing.</param>
    /// <returns>A task that will be completed due to the timer firing, <see cref="M:System.Threading.PeriodicTimer.Dispose" /> being called to stop the timer, or cancellation being requested.</returns>
    public ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken);
}