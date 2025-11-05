using Tiempito.Daemon.Domain.Sessions.Abstractions;

namespace Tiempito.Daemon.Infrastructure.Sessions;

/// <summary>
/// Provides a sealed implementation of <see cref="IPeriodicTimer"/> that wraps a <see cref="PeriodicTimer"/>.
/// </summary>
public sealed class DefaultPeriodicTimer : IPeriodicTimer
{
    private readonly PeriodicTimer _timer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultPeriodicTimer"/> class with the specified <see cref="PeriodicTimer"/>.
    /// </summary>
    /// <param name="timer">The underlying <see cref="PeriodicTimer"/> to wrap.</param>
    public DefaultPeriodicTimer(PeriodicTimer timer)
    {
        _timer = timer;
    }

    /// <inheritdoc/>
    public TimeSpan Period
    {
        get => _timer.Period;
        set => _timer.Period = value;
    }

    /// <inheritdoc/>
    public ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken)
    {
        return _timer.WaitForNextTickAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _timer.Dispose();
    }
}