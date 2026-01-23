using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

using Moq;

using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Sessions;
using Tiempito.Daemon.Domain.Sessions.Abstractions;
using Tiempito.Daemon.Domain.Sessions.Enums;
using Tiempito.Daemon.Infrastructure.Sessions;
using Tiempito.Daemon.Tests.Helpers;
using Tiempito.Daemon.Tests.Sessions.Helpers;

namespace Tiempito.Daemon.Tests.Domain.Sessions;

/// <summary>
/// Unit tests for the <see cref="Session"/> class.
/// </summary>
[Trait("Sessions", "Unit")]
public sealed class SessionTests : IDisposable
{
    private const string SessionId = "sessionId";

    private readonly MockRepository _mockRepository;
    private readonly Mock<ILogger<Session>> _loggerStub;
    private readonly Mock<IPeriodicTimer> _timerMock;
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly SessionConfig _anyConfig = SessionProvider.CreateConfig();
    private readonly Session _session;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionTests"/> class.
    /// </summary>
    public SessionTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Loose);
        _loggerStub = _mockRepository.Create<ILogger<Session>>();
        _timerMock = _mockRepository.Create<IPeriodicTimer>();
        _fakeTimeProvider = new FakeTimeProvider();
        _session = Session.Create(_loggerStub.Object, SessionId, _anyConfig, _timerMock.Object);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Tests that <see cref="Session.Create"/> creates a session with the expected properties.
    /// </summary>
    [Fact]
    public void Create_should_CreateSessionWithExpectedProperties()
    {
        // Act
        Session actual = Session.Create(_loggerStub.Object, SessionId, _anyConfig, _timerMock.Object);

        // Assert
        Assert.NotNull(actual);
        Assert.Same(actual.Id, SessionId);
        Assert.Equal(actual.Configuration, _anyConfig);
    }

    /// <summary>
    /// Tests that calling <see cref="Session.Start"/> changes the session status to <see cref="SessionStatus.Executing"/>.
    /// </summary>
    [Fact]
    public void Start_should_ChangeStatusToExecuting()
    {
        // Act
        _session.Start();

        // Assert
        Assert.Equal(SessionStatus.Executing, _session.State.Status);
    }

    /// <summary>
    /// Tests that calling <see cref="Session.Start"/> sets the timer period to <see cref="Session.SecondInterval"/>.
    /// </summary>
    [Fact]
    public void Start_should_SetTimerPeriodToSecondInterval()
    {
        // Arrange
        _timerMock.Reset();

        // Act
        _session.Start();

        // Assert
        _timerMock.VerifySet(m => m.Period = Session.SecondInterval);
    }

    /// <summary>
    /// Tests that <see cref="Session.Start"/> propagates the provided <see cref="CancellationToken"/> to the timer.
    /// </summary>
    [Fact]
    public void Start_should_PropagateCancellationTokenToTimer()
    {
        // Arrange
        using var tokenSource = new CancellationTokenSource();

        // Act
        _session.Start(tokenSource.Token);

        // Assert
        // ReSharper disable once AccessToDisposedClosure
        _timerMock.Verify(m => m.WaitForNextTickAsync(tokenSource.Token));
    }

    /// <summary>
    /// Tests that calling <see cref="Session.Pause"/> changes the session status to <see cref="SessionStatus.Paused"/>.
    /// </summary>
    [Fact]
    public void Pause_should_ChangeStatusToPaused()
    {
        // Arrange
        _session.Start();

        // Act
        _session.Pause();

        // Assert
        Assert.Equal(SessionStatus.Paused, _session.State.Status);
    }

    /// <summary>
    /// Tests that calling <see cref="Session.Pause"/> sets the timer period to <see cref="Timeout.InfiniteTimeSpan"/>.
    /// </summary>
    [Fact]
    public void Pause_should_SetTimerPeriodToInfinite()
    {
        // Arrange
        _timerMock.Reset();

        // Act
        _session.Pause();

        // Assert
        _timerMock.VerifySet(m => m.Period = Timeout.InfiniteTimeSpan, Times.Once);
    }

    /// <summary>
    /// Tests that calling <see cref="Session.Resume"/> changes the session status to <see cref="SessionStatus.Executing"/>.
    /// </summary>
    [Fact]
    public void Resume_should_ChangeStatusToExecuting()
    {
        // Arrange
        _session.Start();
        _session.Pause();

        // Act
        _session.Resume();

        // Assert
        Assert.Equal(SessionStatus.Executing, _session.State.Status);
    }

    /// <summary>
    /// Tests that calling <see cref="Session.Resume"/> sets the timer period to <see cref="Session.SecondInterval"/>.
    /// </summary>
    [Fact]
    public void Resume_should_SetTimerPeriodToSecondInterval()
    {
        // Arrange
        _timerMock.Reset();

        // Act
        _session.Resume();

        // Assert
        _timerMock.VerifySet(m => m.Period = Session.SecondInterval);
    }

    /// <summary>
    /// Tests that calling <see cref="Session.Cancel"/> changes the session status to <see cref="SessionStatus.Cancelled"/> when the session is executing.
    /// </summary>
    [Fact]
    public void Cancel_should_ChangeStatusToCanceled_when_SessionIsExecuting()
    {
        // Arrange
        _session.Start();

        // Act
        _session.Cancel();

        // Assert
        Assert.Equal(SessionStatus.Cancelled, _session.State.Status);
    }

    /// <summary>
    /// Tests that calling <see cref="Session.Cancel"/> changes the session status to <see cref="SessionStatus.Cancelled"/> when the session is paused.
    /// </summary>
    [Fact]
    public void Cancel_should_ChangeStatusToCanceled_when_SessionIsPaused()
    {
        // Arrange
        _session.Start();
        _session.Pause();

        // Act
        _session.Cancel();

        // Assert
        Assert.Equal(SessionStatus.Cancelled, _session.State.Status);
    }

    /// <summary>
    /// Tests that calling <see cref="Session.Cancel"/> disposes the timer.
    /// </summary>
    [Fact]
    public void Cancel_should_DisposeTimer()
    {
        // Act
        _session.Cancel();

        // Assert
        _timerMock.Verify(m => m.Dispose(), Times.Once);
    }

    /// <summary>
    /// Tests that the <see cref="Session.SecondElapsedAsync"/> event is invoked when a second elapses.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SecondElapsedAsync_should_BeInvoked_when_SecondElapses()
    {
        // Arrange
        Session session = CreateWithFakeDeps(focusDuration: 5, breakDuration: 5);
        using var advancer = new SessionTimeAdvancer(session, _fakeTimeProvider);
        bool invoked = false;
        session.SecondElapsedAsync += _ =>
        {
            invoked = true;
            return ValueTask.CompletedTask;
        };
        session.Start();

        // Act
        await advancer.AdvanceAsync(Session.SecondInterval);
        session.SecondElapsedAsync = null;

        // Assert
        Assert.True(invoked);
    }

    /// <summary>
    /// Tests that the <see cref="Session.IntervalCompletedAsync"/> event is invoked when an interval completes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task IntervalCompletedAsync_should_BeInvoked_when_IntervalCompletes()
    {
        // Arrange
        const int focusDuration = 5;
        Session session = CreateWithFakeDeps(focusDuration, breakDuration: 5);
        using var advancer = new SessionTimeAdvancer(session, _fakeTimeProvider);
        bool invoked = false;
        session.IntervalCompletedAsync += _ =>
        {
            invoked = true;
            return ValueTask.CompletedTask;
        };
        session.Start();

        // Act
        await advancer.AdvanceAsync(TimeSpan.FromSeconds(focusDuration));
        session.IntervalCompletedAsync = null;

        // Assert
        Assert.True(invoked);
    }

    /// <summary>
    /// Tests that the <see cref="Session.CompletedAsync"/> event is invoked and the session status is set to <see cref="SessionStatus.Finished"/>
    /// when the session completes all cycles.
    /// </summary>
    /// <param name="targetCycles">Number of cycles to complete.</param>
    /// <param name="focusDuration">Duration of the focus interval in seconds.</param>
    /// <param name="breakDuration">Duration of the break interval in seconds.</param>
    /// <param name="delayBetweenTimes">Delay between intervals in seconds.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(1, 5, 5, 0)]
    [InlineData(5, 5, 5, 5)]
    public async Task CompletedAsync_should_BeInvokedAndSetStatusToFinished_when_SessionCompletes(
        int targetCycles, int focusDuration, int breakDuration, int delayBetweenTimes)
    {
        // Arrange
        Session session = CreateWithFakeDeps(focusDuration, breakDuration, targetCycles, delayBetweenTimes);
        int secondsToAdvance = targetCycles * (focusDuration + breakDuration + (delayBetweenTimes * 2));
        using var advancer = new SessionTimeAdvancer(session, _fakeTimeProvider);
        bool invoked = false;
        session.CompletedAsync += _ =>
        {
            invoked = true;
            return ValueTask.CompletedTask;
        };
        session.Start();

        // Act
        await advancer.AdvanceAsync(TimeSpan.FromSeconds(secondsToAdvance));
        session.CompletedAsync = null;

        // Assert
        Assert.True(invoked);
        Assert.Equal(SessionStatus.Finished, session.State.Status);
    }

    /// <summary>
    /// Tests that calling <see cref="Session.Dispose"/> disposes the timer.
    /// </summary>
    [Fact]
    public void Dispose_should_DisposeTimer()
    {
        // Act
        _session.Dispose();

        // Assert
        _timerMock.Verify(m => m.Dispose(), Times.Once);
    }

    /// <summary>
    /// Tests that calling <see cref="Session.Dispose"/> sets all event handlers to null.
    /// </summary>
    [Fact]
    public void Dispose_should_SetEventsToNull()
    {
        // Arrange
        _session.SecondElapsedAsync = _ => ValueTask.CompletedTask;
        _session.IntervalCompletedAsync = _ => ValueTask.CompletedTask;
        _session.CompletedAsync = _ => ValueTask.CompletedTask;

        // Act
        _session.Dispose();

        // Assert
        Assert.Null(_session.SecondElapsedAsync);
        Assert.Null(_session.IntervalCompletedAsync);
        Assert.Null(_session.CompletedAsync);
    }

    /// <summary>
    /// Tests that <see cref="Session.Create"/> sets the session status to <see cref="SessionStatus.None"/>.
    /// </summary>
    [Fact]
    public void Create_should_SetSessionStatusToNone()
    {
        Session session = CreateWithFakeDeps(focusDuration: 5, breakDuration: 5);

        // Assert
        Assert.Equal(SessionStatus.None, session.State.Status);
    }

    private Session CreateWithFakeDeps(
        int focusDuration,
        int breakDuration,
        int targetCycles = 1,
        int delayBetweenTimes = 0)
    {
        SessionConfig config = new (
            Id: "fakeConfig",
            TargetCycles: targetCycles,
            DelayBetweenTimes: TimeSpan.FromSeconds(delayBetweenTimes),
            FocusDuration: TimeSpan.FromSeconds(focusDuration),
            BreakDuration: TimeSpan.FromSeconds(breakDuration));

        var innerTimer = new PeriodicTimer(Session.SecondInterval, _fakeTimeProvider);
        var timer = new DefaultPeriodicTimer(innerTimer);
        return Session.Create(_loggerStub.Object, SessionId, config, timer);
    }
}
