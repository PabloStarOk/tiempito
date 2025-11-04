using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Sessions.Enums;
using Tiempito.Daemon.Domain.Sessions.ValueObjects;

namespace Tiempito.Daemon.Domain.Sessions;

/// <summary>
/// Represents a session with timed intervals and state management.
/// </summary>
public sealed class Session : ISession
{
    /// <inheritdoc/>
    public string Id { get; }

    /// <inheritdoc/>
    public SessionConfig Configuration { get; }

    /// <inheritdoc/>
    public SessionState State { get; private set; }

    /// <inheritdoc/>
    public Func<ISession, ValueTask>? SecondElapsedAsync { get; set; }

    /// <inheritdoc/>
    public Func<ISession, ValueTask>? IntervalCompletedAsync { get; set; }

    /// <inheritdoc/>
    public Func<ISession, ValueTask>? CompletedAsync { get; set; }

    private static readonly TimeSpan SecondInterval = TimeSpan.FromSeconds(1);
    private readonly ILogger<Session> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly SessionIntervalType[] _intervalsSequence;
    private Task? _runTask;
    private PeriodicTimer? _timer;
    private int _currentIntervalIndex;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Session"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="id">The unique identifier for the session.</param>
    /// <param name="configuration">The configuration settings for the session.</param>
    /// <param name="state">The initial state of the session.</param>
    /// <param name="timeProvider">The time provider used for interval timing.</param>
    /// <param name="intervalsSequence">The sequence of intervals for the session.</param>
    private Session(
        ILogger<Session> logger,
        string id,
        SessionConfig configuration,
        SessionState state,
        TimeProvider timeProvider,
        SessionIntervalType[] intervalsSequence)
    {
        Id = id;
        State = state;
        Configuration = configuration;
        _logger = logger;
        _timeProvider = timeProvider;
        _intervalsSequence = intervalsSequence;
    }

    /// <summary>
    /// Creates a new <see cref="Session"/> instance with the specified parameters.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="id">The unique identifier for the session.</param>
    /// <param name="configuration">The configuration settings for the session.</param>
    /// <param name="timeProvider">The time provider used for interval timing.</param>
    /// <returns>A new <see cref="Session"/> instance.</returns>
    public static Session Create(
        ILogger<Session> logger,
        string id,
        SessionConfig configuration,
        TimeProvider timeProvider)
    {
        SessionIntervalType[] intervalsSequence = configuration.DelayBetweenTimes > TimeSpan.Zero
            ? [SessionIntervalType.Focus, SessionIntervalType.Delay, SessionIntervalType.Break, SessionIntervalType.Delay]
            : [SessionIntervalType.Focus, SessionIntervalType.Break];

        return new Session(
            logger,
            id,
            configuration,
            state: SessionState.CreateInitial(configuration.FocusDuration),
            timeProvider,
            intervalsSequence);
    }

    /// <inheritdoc/>
    public void Start(CancellationToken cancellationToken = default)
    {
        State = State.WithStatus(SessionStatus.Executing);
        _timer = new PeriodicTimer(SecondInterval, _timeProvider);
        _runTask = RunAsync(cancellationToken);
        _logger.LogDebug("Session {Id}: Started", Id);
    }

    /// <inheritdoc/>
    public async ValueTask CancelAsync()
    {
        State = State.WithStatus(SessionStatus.Cancelled);
        await DisposeAsync();
        _logger.LogDebug("Session {Id}: Canceled", Id);
    }

    /// <inheritdoc/>
    public void Pause()
    {
        State = State.WithStatus(SessionStatus.Paused);
        if (_timer is not null)
        {
            _timer.Period = Timeout.InfiniteTimeSpan;
        }

        _logger.LogDebug("Session {Id}: Paused", Id);
    }

    /// <inheritdoc/>
    public void Resume()
    {
        State = State.WithStatus(SessionStatus.Executing);
        if (_timer is not null)
        {
            _timer.Period = SecondInterval;
        }

        _logger.LogDebug("Session {Id}: Resumed", Id);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _timer?.Dispose();
        _timer = null;
        SecondElapsedAsync = null;
        IntervalCompletedAsync = null;
        CompletedAsync = null;

        if (_runTask is null)
        {
            return;
        }

        try
        {
            await _runTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Session {Id}: An error occurred while disposing the session.", Id);
        }

        _runTask.Dispose();
        _runTask = null;
        _logger.LogDebug("Session {Id}: Disposed.", Id);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        if (_timer is null)
        {
            return;
        }

        try
        {
            while (await _timer.WaitForNextTickAsync(cancellationToken))
            {
                await OnSecondElapsedAsync();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async ValueTask OnSecondElapsedAsync()
    {
        State = State.WithElapsedSecond();
        if (SecondElapsedAsync is not null)
        {
            await SecondElapsedAsync(this);
        }

        if (State.ElapsedTime < State.TargetDuration)
        {
            return;
        }

        if (IntervalCompletedAsync is not null)
        {
            await IntervalCompletedAsync(this);
        }

        (SessionIntervalType nextInterval, TimeSpan nextTargetDuration) = DetermineNextInterval();
        State = State.WithNewInterval(nextInterval, nextTargetDuration);

        if (Configuration.TargetCycles > 0 && State.Cycle >= Configuration.TargetCycles)
        {
            await CompleteAsync();
        }
    }

    private async ValueTask CompleteAsync()
    {
        State = State.WithStatus(SessionStatus.Finished);
        if (CompletedAsync is not null)
        {
            await CompletedAsync(this);
        }

        await DisposeAsync();
        _logger.LogDebug("Session {Id}: Completed", Id);
    }

    private (SessionIntervalType, TimeSpan) DetermineNextInterval()
    {
        _currentIntervalIndex++;
        if (_currentIntervalIndex >= _intervalsSequence.Length)
        {
            _currentIntervalIndex = 0;
        }

        SessionIntervalType nextInterval = _intervalsSequence[_currentIntervalIndex];

        TimeSpan targetDuration = nextInterval switch
        {
            SessionIntervalType.Focus => Configuration.FocusDuration,
            SessionIntervalType.Break => Configuration.BreakDuration,
            SessionIntervalType.Delay => Configuration.DelayBetweenTimes,
            _ => throw new InvalidOperationException("Invalid interval type.")
        };

        return (nextInterval, targetDuration);
    }
}
