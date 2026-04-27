using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Sessions.Abstractions;
using Tiempito.Daemon.Domain.Sessions.Enums;
using Tiempito.Daemon.Domain.Sessions.ValueObjects;

namespace Tiempito.Daemon.Domain.Sessions;

/// <summary>
/// Represents a session with timed intervals and state management.
/// </summary>
public sealed class Session : ISession
{
    /// <summary>
    /// The interval of one second used for session timing.
    /// </summary>
    public static readonly TimeSpan SecondInterval = TimeSpan.FromSeconds(1);

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

    private readonly ILogger<Session> _logger;
    private readonly IPeriodicTimer _timer;
    private readonly SessionIntervalType[] _intervalsSequence;
    private Task? _runTask;
    private int _currentIntervalIndex;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Session"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="id">The unique identifier for the session.</param>
    /// <param name="configuration">The configuration settings for the session.</param>
    /// <param name="state">The initial state of the session.</param>
    /// <param name="timer">The periodic timer used for interval timing.</param>
    /// <param name="intervalsSequence">The sequence of intervals for the session.</param>
    private Session(
        ILogger<Session> logger,
        string id,
        SessionConfig configuration,
        SessionState state,
        IPeriodicTimer timer,
        SessionIntervalType[] intervalsSequence)
    {
        Id = id;
        State = state;
        Configuration = configuration;
        _logger = logger;
        _timer = timer;
        _intervalsSequence = intervalsSequence;
    }

    /// <summary>
    /// Creates a new <see cref="Session"/> instance with the specified parameters.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="id">The unique identifier for the session.</param>
    /// <param name="configuration">The configuration settings for the session.</param>
    /// <param name="timer">The periodic timer used for interval timing.</param>
    /// <returns>A new <see cref="Session"/> instance.</returns>
    public static Session Create(
        ILogger<Session> logger,
        string id,
        SessionConfig configuration,
        IPeriodicTimer timer)
    {
        timer.Period = Timeout.InfiniteTimeSpan;

        SessionIntervalType[] intervalsSequence = configuration.DelayBetweenTimes > TimeSpan.Zero
            ? [SessionIntervalType.Focus, SessionIntervalType.Delay, SessionIntervalType.Break, SessionIntervalType.Delay]
            : [SessionIntervalType.Focus, SessionIntervalType.Break];

        return new Session(
            logger,
            id,
            configuration,
            state: SessionState.CreateInitial(configuration.FocusDuration),
            timer,
            intervalsSequence);
    }

    /// <inheritdoc/>
    public void Start(CancellationToken cancellationToken = default)
    {
        if (_disposed || State.Status is not SessionStatus.None)
        {
            return;
        }

        State = State.WithStatus(SessionStatus.Executing);
        _timer.Period = SecondInterval;
        _runTask = RunAsync(cancellationToken);
        _logger.LogDebug("Session {Id}: Started", Id);
    }

    /// <inheritdoc/>
    public void Cancel()
    {
        if (_disposed || State.Status is SessionStatus.Cancelled or SessionStatus.Finished)
        {
            return;
        }

        State = State.WithStatus(SessionStatus.Cancelled);
        _timer.Dispose();
        _logger.LogDebug("Session {Id}: Canceled", Id);
    }

    /// <inheritdoc/>
    public void Pause()
    {
        if (_disposed || State.Status is not SessionStatus.Executing)
        {
            return;
        }

        State = State.WithStatus(SessionStatus.Paused);
        _timer.Period = Timeout.InfiniteTimeSpan;
        _logger.LogDebug("Session {Id}: Paused", Id);
    }

    /// <inheritdoc/>
    public void Resume()
    {
        if (_disposed || State.Status is not SessionStatus.Paused)
        {
            return;
        }

        State = State.WithStatus(SessionStatus.Executing);
        _timer.Period = SecondInterval;
        _logger.LogDebug("Session {Id}: Resumed", Id);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (State.Status is not SessionStatus.Cancelled and not SessionStatus.Finished)
        {
            State = State.WithStatus(SessionStatus.Cancelled);
        }

        _timer.Dispose();
        SecondElapsedAsync = null;
        IntervalCompletedAsync = null;
        CompletedAsync = null;

        if (_runTask is not null && _runTask.IsCompleted)
        {
            _runTask.Dispose();
            _logger.LogDebug("Session {Id}: RunAsync task disposed", Id);
        }

        _runTask = null;
        _logger.LogDebug("Session {Id}: Disposed.", Id);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Session {Id}: RunAsync task started", Id);
        try
        {
            while (await _timer.WaitForNextTickAsync(cancellationToken))
            {
                await OnSecondElapsedAsync();
            }

            _logger.LogDebug("Session {Id}: RunAsync task completed", Id);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Session {Id}: RunAsync task canceled", Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Session {Id}: An exception occurred while running.", Id);
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
        _timer.Dispose();
        if (CompletedAsync is not null)
        {
            await CompletedAsync(this);
        }

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
