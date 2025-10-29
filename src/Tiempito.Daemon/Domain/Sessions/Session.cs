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
    private readonly ILogger<Session> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly SessionIntervalType[] _intervalsSequence;
    private readonly Func<Session, ValueTask> _onSecondElapsedAsync;
    private readonly Func<Session, ValueTask> _onIntervalCompletedAsync;
    private readonly Func<Session, ValueTask> _onSessionCompletedAsync;
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
    /// <param name="onSecondElapsedAsync">Callback invoked when a second elapses.</param>
    /// <param name="onIntervalCompletedAsync">Callback invoked when an interval is completed.</param>
    /// <param name="onSessionCompletedAsync">Callback invoked when the session is completed.</param>
    private Session(
        ILogger<Session> logger,
        string id,
        SessionConfig configuration,
        SessionState state,
        TimeProvider timeProvider,
        SessionIntervalType[] intervalsSequence,
        Func<Session, ValueTask> onSecondElapsedAsync,
        Func<Session, ValueTask> onIntervalCompletedAsync,
        Func<Session, ValueTask> onSessionCompletedAsync)
    {
        Id = id;
        Configuration = configuration;
        State = state;
        _logger = logger;
        _timeProvider = timeProvider;
        _intervalsSequence = intervalsSequence;
        _onSecondElapsedAsync = onSecondElapsedAsync;
        _onIntervalCompletedAsync = onIntervalCompletedAsync;
        _onSessionCompletedAsync = onSessionCompletedAsync;
    }

    /// <summary>
    /// Creates a new <see cref="Session"/> instance with the specified parameters.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="id">The unique identifier for the session.</param>
    /// <param name="configuration">The configuration settings for the session.</param>
    /// <param name="timeProvider">The time provider used for interval timing.</param>
    /// <param name="onSecondElapsed">Callback invoked when a second elapses.</param>
    /// <param name="onIntervalCompleted">Callback invoked when an interval is completed.</param>
    /// <param name="onSessionCompleted">Callback invoked when the session is completed.</param>
    /// <returns>A new <see cref="Session"/> instance.</returns>
    public static Session Create(
        ILogger<Session> logger,
        string id,
        SessionConfig configuration,
        TimeProvider timeProvider,
        Func<Session, ValueTask> onSecondElapsed,
        Func<Session, ValueTask> onIntervalCompleted,
        Func<Session, ValueTask> onSessionCompleted)
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
            intervalsSequence,
            onSecondElapsed,
            onIntervalCompleted,
            onSessionCompleted);
    }

    /// <summary>
    /// Starts the session.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to stop the session.</param>
    public void Start(CancellationToken cancellationToken = default)
    {
        _timer = new PeriodicTimer(SecondInterval, _timeProvider);
        State = State.WithStatus(SessionStatus.Executing);
        _runTask = RunAsync(cancellationToken);
    }

    /// <summary>
    /// Cancels the session and releases resources.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> that completes when cancellation and cleanup are finished.</returns>
    public async ValueTask CancelAsync()
    {
        await DisposeAsync();
        State = State.WithStatus(SessionStatus.Cancelled);
    }

    /// <summary>
    /// Pauses the session.
    /// </summary>
    public void Pause()
    {
        if (_timer is not null)
        {
            _timer.Period = Timeout.InfiniteTimeSpan;
        }

        State = State.WithStatus(SessionStatus.Paused);
    }

    /// <summary>
    /// Resumes the session.
    /// </summary>
    public void Resume()
    {
        if (_timer is not null)
        {
            _timer.Period = SecondInterval;
        }

        State = State.WithStatus(SessionStatus.Executing);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _timer?.Dispose();
        _timer = null;

        try
        {
            _runTask?.GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Session {Id}: An error occurred while disposing the session.", Id);
        }

        _runTask?.Dispose();
        _runTask = null;
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
        await _onSecondElapsedAsync(this);

        if (State.ElapsedTime < State.TargetDuration)
        {
            return;
        }

        await _onIntervalCompletedAsync(this);
        (SessionIntervalType nextInterval, TimeSpan nextTargetDuration) = DetermineNextInterval();
        State = State.WithNewInterval(nextInterval, nextTargetDuration);

        if (Configuration.TargetCycles > 0 && State.Cycle >= Configuration.TargetCycles)
        {
            await CompleteAsync();
        }
    }

    private async ValueTask CompleteAsync()
    {
        await DisposeAsync();
        await _onSessionCompletedAsync(this);
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
