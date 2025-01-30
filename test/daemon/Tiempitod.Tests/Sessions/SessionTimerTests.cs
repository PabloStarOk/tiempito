using Microsoft.Extensions.Time.Testing;
using Moq;
using Tiempitod.NET.Session;

namespace Tiempitod.Tests.Sessions;

[Trait("Sessions", "Unit")]
public class SessionTimerTests : IDisposable
{
    private readonly SessionTimer _sessionTimer;
    private readonly MockRepository _mockRepository;
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly Mock<IProgress<Session>> _timeProgressMock;
    private readonly Mock<ISessionStorage> _sessionStorageMock;
    private readonly CancellationTokenSource _tokenSource;
    private Session _session;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(1);
    private const int TargetCycles = 1;
    private readonly TimeSpan _delayBetweenTimes = TimeSpan.FromSeconds(2);
    private readonly TimeSpan _focusDuration = TimeSpan.FromSeconds(10);
    private readonly TimeSpan _breakDuration = TimeSpan.FromSeconds(5);

    public SessionTimerTests()
    {
        _fakeTimeProvider = new FakeTimeProvider();
        _mockRepository = new MockRepository(MockBehavior.Strict);
        _timeProgressMock = _mockRepository.Create<IProgress<Session>>();
        _sessionStorageMock = _mockRepository.Create<ISessionStorage>();
        _tokenSource = new CancellationTokenSource();
        
        _sessionTimer = new SessionTimer(_fakeTimeProvider, _timeProgressMock.Object, _sessionStorageMock.Object, _interval);
        
        _session = new Session("TestSession", TargetCycles, _delayBetweenTimes, _focusDuration, _breakDuration);
    }

    public void Dispose()
    {
        _mockRepository.VerifyAll();
        _tokenSource.Dispose();
    }
    
    [Fact]
    public void Start_should_RaiseOnStartedSession_when_SessionStorageReturnsTrue()
    { 
        var startedEventRaised = false;
        
        SetupMethods(addSession: true);
        _sessionTimer.OnSessionStarted += (sender, _) =>
        {   
            Assert.Same(sender, _sessionTimer);
            startedEventRaised = true;
            return Task.CompletedTask;
        };

        _sessionTimer.Start(_session, _tokenSource.Token);
        
        Assert.True(startedEventRaised);
    }

    [Fact]
    public void Start_should_NotRaiseOnStartedSession_when_SessionStorageReturnsFalse()
    {
        _sessionStorageMock.Setup(
                s => s.AddSession(SessionStatus.Executing, _session))
            .Returns(false);
        _sessionTimer.OnSessionStarted += (_, _) =>
        {
            Assert.Fail("OnSessionStarted event shouldn't be raised.");
            return Task.CompletedTask;
        };
        
        _sessionTimer.Start(_session, _tokenSource.Token);
    }

    [Fact]
    public void Stop_should_StopAndReturnSession()
    {
        SetupMethods(addSession: true, removeSession: true);
        _sessionTimer.Start(_session, _tokenSource.Token);
        
        Session stoppedSession = _sessionTimer.Stop(_session.Id);
        
        Assert.Equal(_session, stoppedSession);
    }
    
    [Theory]
    [MemberData(nameof(GetRandomSessions))]
    public void StopAll_should_StopAndReturnAllSessions(Session rndSession1, Session rndSession2)
    {
        var fixedSession = new Session("FixedSession", TargetCycles, _delayBetweenTimes, _focusDuration, _breakDuration);
        Session[] targetSessions = [_session, rndSession1, fixedSession, rndSession2];
        
        _sessionStorageMock.Setup(
                s => s.AddSession(SessionStatus.Executing, 
                    It.IsIn(_session, rndSession1, fixedSession, rndSession2)))
                .Returns(true);
        _sessionStorageMock.Setup(s => s.RemoveSession(SessionStatus.Executing, 
            It.IsIn(_session.Id, rndSession1.Id, fixedSession.Id, rndSession2.Id)))
            .Returns((SessionStatus _, string id) => targetSessions.First(s => s.Id == id));
        _sessionTimer.Start(_session, _tokenSource.Token);
        _sessionTimer.Start(rndSession1, _tokenSource.Token);
        _sessionTimer.Start(fixedSession, _tokenSource.Token);
        _sessionTimer.Start(rndSession2, _tokenSource.Token);

        Session[] stoppedSessions = _sessionTimer.StopAll();
        
        Assert.NotEmpty(stoppedSessions);
        Assert.Equal(targetSessions, stoppedSessions);
    }
    
    [Fact]
    public void SessionTimer_should_RemoveSessions_when_TokenIsCancelled()
    {
        SetupMethods(runningSessions: true, addSession: true, updateSession: true,
            removeSession: true, timeProgressReport: true); // RemoveSession() must be verified.
        _sessionTimer.Start(_session, _tokenSource.Token);
        _fakeTimeProvider.Advance(_interval);
        
        _tokenSource.Cancel();
    }
    
    [Fact]
    public void SessionTimer_should_Report_when_SessionIsRunning()
    {
        SetupMethods(runningSessions: true, addSession: true, updateSession: true, 
            timeProgressReport: true);
        _sessionTimer.Start(_session, _tokenSource.Token);
        
        _fakeTimeProvider.Advance(_interval);
    }
    
    [Fact]
    public void SessionTimer_should_RaiseOnTimeCompleted_when_SessionTimeIsCompleted()
    {
        var eventRaised = false;
        _session.Elapsed += _focusDuration;
        SetupMethods(runningSessions: true, addSession: true, updateSession: true, 
            timeProgressReport: true);
        _sessionTimer.Start(_session, _tokenSource.Token);
        
        _sessionTimer.OnTimeCompleted += (sender, timeType) =>
        {
            Assert.Same(sender, _sessionTimer);
            Assert.Equal(timeType, _session.CurrentTimeType);
            eventRaised = true;
            return Task.CompletedTask;
        };
        _fakeTimeProvider.Advance(_focusDuration);
        
        Assert.True(eventRaised);
    }
    
    [Fact]
    public void SessionTimer_should_RaiseOnSessionCompleted_when_SessionTimeIsCompleted()
    {
        var eventRaised = false;
        
        _session.CurrentCycle = TargetCycles;
        _session.Elapsed = _focusDuration;
        SetupMethods(runningSessions: true, addSession: true, updateSession: true,
            removeSession: true, timeProgressReport: true);
        _sessionTimer.Start(_session, _tokenSource.Token);
        
        _sessionTimer.OnSessionCompleted += (sender, session) =>
        {
            Assert.Same(sender, _sessionTimer);
            Assert.Equal(session, _session);
            eventRaised = true;
            return Task.CompletedTask;
        };
        _fakeTimeProvider.Advance(_interval);
        
        Assert.True(eventRaised);
    }
    
    [Fact]
    public void SessionTimer_should_RaiseOnDelayElapsed_when_DelaySecondElapses()
    {
        var eventRaised = false; // 0 is infinite so session will never be completed.

        _session.CurrentTimeType = TimeType.Focus;
        _session.Elapsed = _focusDuration;
        SetupMethods(runningSessions: true, addSession: true, updateSession: true,
            timeProgressReport: true);
        _sessionTimer.Start(_session, _tokenSource.Token);
        
        _sessionTimer.OnDelayElapsed += (sender, _) =>
        {
            Assert.Same(sender, _sessionTimer);
            eventRaised = true;
            return Task.CompletedTask;
        };
        _fakeTimeProvider.Advance(_delayBetweenTimes);
        
        Assert.True(eventRaised);
    }
    
    [Fact]
    public void SessionTimer_should_NotRaiseOnSessionCompleted_when_TargetCyclesIsZero()
    { 
        var testSession = new Session(_session.Id, 0, _delayBetweenTimes, _focusDuration, _breakDuration)
        {
            CurrentCycle = 1,
            Elapsed = _focusDuration
        };

        _sessionStorageMock.Setup(s => s.RunningSessions[testSession.Id]).Returns(testSession);
        _sessionStorageMock.Setup(s => s.AddSession(SessionStatus.Executing, testSession)).Returns(true);
        SetupMethods(updateSession: true, timeProgressReport: true);
        _sessionTimer.Start(testSession, _tokenSource.Token);
        
        _sessionTimer.OnSessionCompleted += (_, _) =>
        {
            Assert.Fail();
            return Task.CompletedTask;
        };
        _fakeTimeProvider.Advance(_focusDuration);
    }
    
    [Fact]
    public void SessionTimer_should_NotRaiseOnDelayElapsed_when_DelayIsZero()
    {
        TimeSpan delayBetweenTimes = TimeSpan.Zero;
        var testSession = new Session(_session.Id, TargetCycles, delayBetweenTimes, _focusDuration, _breakDuration)
        {
            Elapsed = _focusDuration // Focus time is completed.
        };

        _sessionStorageMock.Setup(s => s.RunningSessions[testSession.Id]).Returns(testSession);
        _sessionStorageMock.Setup(s => s.AddSession(SessionStatus.Executing, testSession)).Returns(true);
        SetupMethods(updateSession: true, timeProgressReport: true);
        _sessionTimer.Start(testSession, _tokenSource.Token);
        
        _sessionTimer.OnDelayElapsed += (_, _) =>
        {
            Assert.Fail();
            return Task.CompletedTask;
        };
        _fakeTimeProvider.Advance(_focusDuration);
    }
    
    /// <summary>
    /// Set up the methods according to the given booleans.
    /// </summary>
    /// <param name="runningSessions">True if <see cref="ISessionStorage.RunningSessions"/> dictionary of <see cref="ISessionStorage"/> must be set up.</param>
    /// <param name="addSession">True if <see cref="ISessionStorage.AddSession"/> method of <see cref="ISessionStorage"/> must be set up.</param>
    /// <param name="updateSession">True if <see cref="ISessionStorage.UpdateSession"/> method of <see cref="ISessionStorage"/> must be set up.</param>
    /// <param name="removeSession">True if <see cref="ISessionStorage.RemoveSession"/> method of <see cref="ISessionStorage"/> must be set up.</param>
    /// <param name="timeProgressReport">True if <see cref="IProgress{T}.Report"/> method of <see cref="IProgress{T}"/> must be set up.</param>
    private void SetupMethods(
        bool runningSessions = false, bool addSession = false,
        bool updateSession = false, bool removeSession = false,
        bool timeProgressReport = false)
    {
        if (runningSessions)
            _sessionStorageMock.Setup(s => s.RunningSessions[_session.Id]).Returns(_session);
        
        if (addSession)
            _sessionStorageMock.Setup(s => s.AddSession(SessionStatus.Executing, _session)).Returns(true);
        
        if (updateSession)
            _sessionStorageMock.Setup(s => s.UpdateSession(SessionStatus.Executing, It.IsAny<Session>()));
        
        if (removeSession)
            _sessionStorageMock.Setup(s => s.RemoveSession(SessionStatus.Executing, _session.Id)).Returns(_session);
        
        if (timeProgressReport)
            _timeProgressMock.Setup(t => t.Report(It.IsAny<Session>()));
    }

    /// <summary>
    /// Returns an object which contains two sessions with random data.
    /// </summary>
    /// <returns>An <see cref="object"/> with two sessions.</returns>
    public static IEnumerable<object[]> GetRandomSessions()
    {
        var i = 0;
        while (i < 5)
        {
            yield return [CreateRandomSession(), CreateRandomSession()];
            i++;
        }
    }
    
    /// <summary>
    /// Creates a session with random data.
    /// </summary>
    /// <returns>A <see cref="Session"/>.</returns>
    private static Session CreateRandomSession()
    {
        Random random = Random.Shared;
        
        return new Session(
            id: Guid.NewGuid().ToString(),
            targetCycles: random.Next(0, 20),
            delayBetweenTimes: TimeSpan.FromSeconds(random.Next(0, 1000)),
            focusDuration: TimeSpan.FromSeconds(random.Next(1, 1000)),
            breakDuration: TimeSpan.FromSeconds(random.Next(1, 1000)));
    }
}
