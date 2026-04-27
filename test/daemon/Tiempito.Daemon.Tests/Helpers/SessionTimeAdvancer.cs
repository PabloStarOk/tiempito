using Microsoft.Extensions.Time.Testing;

using Tiempito.Daemon.Domain.Sessions;

namespace Tiempito.Daemon.Tests.Helpers;

/// <summary>
/// Helper class to advance the session time in tests using a fake time provider.
/// </summary>
public sealed class SessionTimeAdvancer : IDisposable
{
    private static readonly TimeSpan TickSignalTimeout = TimeSpan.FromSeconds(1);

    private readonly Session _session;
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly SemaphoreSlim _tickSignal = new (0);

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionTimeAdvancer"/> class.
    /// </summary>
    /// <param name="session">The session to advance time for.</param>
    /// <param name="fakeTimeProvider">The fake time provider used to simulate time advancement.</param>
    public SessionTimeAdvancer(Session session, FakeTimeProvider fakeTimeProvider)
    {
        _session = session;
        _fakeTimeProvider = fakeTimeProvider;
    }

    /// <summary>
    /// Advances the session time by the specified interval, simulating each second using the fake time provider.
    /// </summary>
    /// <param name="interval">The total time interval to advance.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public async ValueTask AdvanceAsync(TimeSpan interval)
    {
        _session.SecondElapsedAsync += OnSecondElapsedAsync;
        for (int i = 0; i < interval.TotalSeconds; i++)
        {
            _fakeTimeProvider.Advance(Session.SecondInterval);
            await _tickSignal.WaitAsync(TickSignalTimeout);
        }

        _session.SecondElapsedAsync -= OnSecondElapsedAsync;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _session.SecondElapsedAsync -= OnSecondElapsedAsync;
        _tickSignal.Dispose();
    }

    private ValueTask OnSecondElapsedAsync(ISession _)
    {
        _tickSignal.Release();
        return ValueTask.CompletedTask;
    }
}