using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Sessions.Enums;
using Tiempito.Daemon.Domain.Sessions.ValueObjects;

namespace Tiempito.Daemon.Domain.Sessions;

/// <summary>
/// Represents a session with timed intervals and state management.
/// </summary>
public sealed class Session : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the unique identifier for the session.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the configuration settings for the session.
    /// </summary>
    public SessionConfig Configuration { get; }

    /// <summary>
    /// Gets the current state of the session.
    /// </summary>
    public SessionState State { get; private set; }

    private static readonly TimeSpan SecondInterval = TimeSpan.FromSeconds(1);
    private readonly TimeProvider _timeProvider;
    private readonly SessionIntervalType[] _intervalsSequence;
    private readonly Action<Session> _onSecondElapsed;
    private readonly Action<Session> _onIntervalCompleted;
    private readonly Action<Session> _onSessionCompleted;
    private ITimer? _timer;
    private int _currentIntervalIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="Session"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the session.</param>
    /// <param name="configuration">The configuration settings for the session.</param>
    /// <param name="state">The initial state of the session.</param>
    /// <param name="timeProvider">The time provider used for interval timing.</param>
    /// <param name="intervalsSequence">The sequence of intervals for the session.</param>
    /// <param name="onSecondElapsed">Callback invoked when a second elapses.</param>
    /// <param name="onIntervalCompleted">Callback invoked when an interval is completed.</param>
    /// <param name="onSessionCompleted">Callback invoked when the session is completed.</param>
    private Session(
        string id,
        SessionConfig configuration,
        SessionState state,
        TimeProvider timeProvider,
        SessionIntervalType[] intervalsSequence,
        Action<Session> onSecondElapsed,
        Action<Session> onIntervalCompleted,
        Action<Session> onSessionCompleted)
    {
        Id = id;
        Configuration = configuration;
        State = state;
        _timeProvider = timeProvider;
        _intervalsSequence = intervalsSequence;
        _onSecondElapsed = onSecondElapsed;
        _onIntervalCompleted = onIntervalCompleted;
        _onSessionCompleted = onSessionCompleted;
    }

    /// <summary>
    /// Creates a new <see cref="Session"/> instance with the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier for the session.</param>
    /// <param name="configuration">The configuration settings for the session.</param>
    /// <param name="timeProvider">The time provider used for interval timing.</param>
    /// <param name="onSecondElapsed">Callback invoked when a second elapses.</param>
    /// <param name="onIntervalCompleted">Callback invoked when an interval is completed.</param>
    /// <param name="onSessionCompleted">Callback invoked when the session is completed.</param>
    /// <returns>A new <see cref="Session"/> instance.</returns>
    public static Session Create(
        string id,
        SessionConfig configuration,
        TimeProvider timeProvider,
        Action<Session> onSecondElapsed,
        Action<Session> onIntervalCompleted,
        Action<Session> onSessionCompleted)
    {
        SessionIntervalType[] intervalsSequence = configuration.DelayBetweenTimes > TimeSpan.Zero
            ? [SessionIntervalType.Focus, SessionIntervalType.Delay, SessionIntervalType.Break, SessionIntervalType.Delay]
            : [SessionIntervalType.Focus, SessionIntervalType.Break];

        return new Session(
            id,
            configuration,
            state: SessionState.CreateInitial(configuration.FocusDuration),
            timeProvider,
            intervalsSequence,
            onSecondElapsed,
            onIntervalCompleted,
            onSessionCompleted);
    }

    /// <summary>
    /// Starts the session.
    /// </summary>
    public void Start()
    {
        _timer = _timeProvider.CreateTimer(OnSecondElapsed, state: null, SecondInterval, SecondInterval);
        State = State.WithStatus(SessionStatus.Executing);
    }

    /// <summary>
    /// Asynchronously cancels the session.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public async ValueTask CancelAsync()
    {
        if (_timer is not null)
        {
            await _timer.DisposeAsync();
            _timer = null;
        }

        State = State.WithStatus(SessionStatus.Cancelled);
    }

    /// <summary>
    /// Pauses the session.
    /// </summary>
    public void Pause()
    {
        _timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        State = State.WithStatus(SessionStatus.Paused);
    }

    /// <summary>
    /// Resumes the session.
    /// </summary>
    public void Resume()
    {
        _timer?.Change(SecondInterval, SecondInterval);
        State = State.WithStatus(SessionStatus.Executing);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _timer?.Dispose();
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_timer is not null)
        {
            await _timer.DisposeAsync();
        }
    }

    private void OnSecondElapsed(object? state)
    {
        State = State.WithElapsedSecond();
        _onSecondElapsed(this);

        if (State.ElapsedTime < State.TargetDuration)
        {
            return;
        }

        _onIntervalCompleted(this);
        (SessionIntervalType nextInterval, TimeSpan nextTargetDuration) = DetermineNextInterval();
        State = State.WithNewInterval(nextInterval, nextTargetDuration);

        if (Configuration.TargetCycles > 0 && State.Cycle >= Configuration.TargetCycles)
        {
            Complete();
        }
    }

    private void Complete()
    {
        _timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        _onSessionCompleted(this);
        State = State.WithStatus(SessionStatus.Finished);
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
