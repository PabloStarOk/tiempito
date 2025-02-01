using AsyncEvent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Tiempito.Daemon.Common;
using Tiempito.Daemon.Configuration.Daemon.Objects;
using Tiempito.Daemon.Configuration.Session.Interfaces;
using Tiempito.Daemon.Configuration.Session.Objects;
using Tiempito.Daemon.Notifications.Enums;
using Tiempito.Daemon.Notifications.Interfaces;
using Tiempito.Daemon.Server.StandardOut;
using Tiempito.Daemon.Sessions;
using Tiempito.Daemon.Sessions.Enums;
using Tiempito.Daemon.Sessions.Interfaces;
using Tiempito.Daemon.Sessions.Objects;
using Tiempito.Daemon.Tests.Sessions.Helpers;

namespace Tiempito.Daemon.Tests.Sessions;

[Trait("Sessions", "Unit")]
public class SessionServiceTests : IDisposable
{
    private readonly SessionService _sessionService;
    private readonly MockRepository _mockRepository;
    private readonly Mock<ISessionConfigService> _sessionConfigServiceMock;
    private readonly Mock<INotificationService> _notificationManagerMock;
    private readonly Progress<Session> _progress;
    private readonly Mock<ISessionStorage> _sessionStorageMock;
    private readonly Mock<ISessionTimer> _sessionTimerMock;
    private readonly Mock<IStandardOutQueue> _stdOutQueueMock;
    private readonly NotificationConfig _notificationConfig;
    
    public SessionServiceTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Strict);
        
        Mock<ILogger<SessionService>> loggerMock = _mockRepository.Create<ILogger<SessionService>>();
        _sessionConfigServiceMock = _mockRepository.Create<ISessionConfigService>();
        Mock<IOptions<NotificationConfig>> notificationOptionsMock = _mockRepository.Create<IOptions<NotificationConfig>>();
        _notificationManagerMock = _mockRepository.Create<INotificationService>();
        _progress = new Progress<Session>();
        _sessionStorageMock = _mockRepository.Create<ISessionStorage>();
        _sessionTimerMock = _mockRepository.Create<ISessionTimer>();
        _stdOutQueueMock = _mockRepository.Create<IStandardOutQueue>();

        _notificationConfig = new NotificationConfig();
        notificationOptionsMock.Setup(n => n.Value).Returns(_notificationConfig);
        
        _sessionService = new SessionService(
            loggerMock.Object,
            _sessionConfigServiceMock.Object,
            notificationOptionsMock.Object,
            _notificationManagerMock.Object,
            _progress,
            _sessionStorageMock.Object,
            _sessionTimerMock.Object,
            _stdOutQueueMock.Object
            );
    }

    public void Dispose()
    {
        // Global Arrange
        _mockRepository.VerifyAll();
    }
    
    #region Service Methods

    [Fact]
    public async Task StartService_should_SubscribeToEvents()
    {
        bool startServiceResult = await _sessionService.StartServiceAsync();
        
        _sessionTimerMock.VerifyAdd(m => m.OnTimeCompleted += It.IsAny<AsyncEventHandler<TimeType>>(), Times.Once);
        _sessionTimerMock.VerifyAdd(m => m.OnDelayElapsed += It.IsAny<AsyncEventHandler<TimeSpan>>(), Times.Once);
        _sessionTimerMock.VerifyAdd(m => m.OnSessionStarted += It.IsAny<AsyncEventHandler>(), Times.Once);
        _sessionTimerMock.VerifyAdd(m => m.OnSessionCompleted += It.IsAny<AsyncEventHandler<Session>>(), Times.Once);
        Assert.True(startServiceResult);
    }
    
    [Fact]
    public async Task StopService_should_UnsubscribeFromEventsAndStopTimers()
    {
        _sessionTimerMock.Setup(m => m.StopAll()).Returns([]);
        
        bool stopServiceResult = await _sessionService.StopServiceAsync();
        
        _sessionTimerMock.VerifyAdd(m => m.OnTimeCompleted += It.IsAny<AsyncEventHandler<TimeType>>(), Times.Never);
        _sessionTimerMock.VerifyAdd(m => m.OnDelayElapsed += It.IsAny<AsyncEventHandler<TimeSpan>>(), Times.Never);
        _sessionTimerMock.VerifyAdd(m => m.OnSessionStarted += It.IsAny<AsyncEventHandler>(), Times.Never);
        _sessionTimerMock.VerifyAdd(m => m.OnSessionCompleted += It.IsAny<AsyncEventHandler<Session>>(), Times.Never);
        Assert.True(stopServiceResult);
    }

    #endregion
    
    #region StartSession
    
    [Theory]
    [MemberData(nameof(GetSessionWithRandomConfig), true, true)]
    [MemberData(nameof(GetSessionWithRandomConfig), true, false)]
    [MemberData(nameof(GetSessionWithRandomConfig), false, true)]
    [MemberData(nameof(GetSessionWithRandomConfig), false, false)]
    public void StartSession_should_StartSession(
        SessionConfig config, Session session, bool specifySessionId, bool specifyConfigId)
    {
        // Arrange
        if (specifyConfigId)
        {
            _sessionConfigServiceMock.Setup(m => m.TryGetConfigById(config.Id, out config))
                .Returns(true);
        }
        else
            _sessionConfigServiceMock.Setup(m => m.DefaultConfig).Returns(config);
        _sessionTimerMock.Setup(m => m.Start(session, It.IsAny<CancellationToken>()));
        _sessionStorageMock.Setup(m => m.RunningSessions).Returns(new Dictionary<string, Session>());
        _sessionStorageMock.Setup(m => m.PausedSessions).Returns(new Dictionary<string, Session>());

        // Act
        OperationResult operationResult = specifySessionId switch
        {
            // Both IDs specified.
            true when specifyConfigId => _sessionService.StartSession(session.Id, config.Id),
            // Only session ID specified.
            true when !specifyConfigId => _sessionService.StartSession(session.Id),
            // Only config ID specified.
            false when specifyConfigId => _sessionService.StartSession(sessionConfigId: config.Id),
            _ => _sessionService.StartSession()
        };
        
        // Assert
        Assert.True(operationResult.Success);
    }

    [Fact]
    public void StartSession_should_ReturnFailedResult_when_ConfigIdNotExists()
    {
        SessionConfig config = SessionProvider.CreateRandomConfig();
        
        _sessionConfigServiceMock.Setup(m => m.TryGetConfigById(It.IsAny<string>(), out config))
            .Returns(false);
        
        OperationResult operationResult = _sessionService.StartSession(sessionConfigId: config.Id);
        
        Assert.False(operationResult.Success);
    }
    
    [Fact]
    public void StartSession_should_ReturnFailedResult_when_SessionIdAlreadyExists()
    {
        SessionConfig config = SessionProvider.CreateConfig();
        Session session = SessionProvider.CreateRandom();
        Dictionary<string, Session> runningSessions = CreateSessionsDictionary(session);
        
        _sessionConfigServiceMock.Setup(m => m.DefaultConfig).Returns(config);
        _sessionStorageMock.Setup(m => m.RunningSessions).Returns(runningSessions);
        _sessionStorageMock.Setup(m => m.PausedSessions).Returns(new Dictionary<string, Session>());
        
        OperationResult operationResult = _sessionService.StartSession(session.Id);
        
        Assert.False(operationResult.Success);
    }
    
    #endregion
    
    #region PauseSession

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PauseSession_should_PauseSession(
        bool specifySessionId)
    {
        Session session = SessionProvider.Create();
        Dictionary<string, Session> runningSessions = CreateSessionsDictionary(session);
        string sessionId = specifySessionId ? session.Id : string.Empty;
        
        _sessionStorageMock.Setup(m => m.RunningSessions).Returns(runningSessions);
        _sessionTimerMock.Setup(m => m.Stop(session.Id)).Returns(session);
        _sessionStorageMock.Setup(m => m.AddSession(SessionStatus.Paused, session)).Returns(true);
        
        OperationResult operationResult = _sessionService.PauseSession(sessionId);
     
        Assert.True(operationResult.Success);
    }
    
    [Fact]
    public void PauseSession_should_ReturnErrorResult_when_ThereAreNoSessionsToPause()
    {
        _sessionStorageMock.Setup(m => m.RunningSessions).Returns(new Dictionary<string, Session>());
        
        OperationResult operationResult = _sessionService.PauseSession();
     
        Assert.False(operationResult.Success);
    }
    
    [Fact]
    public void PauseSession_should_ReturnErrorResult_when_SessionIdNotFound()
    {
        Session session = SessionProvider.Create();
        Dictionary<string, Session> runningSessions = CreateSessionsDictionary(session);
        string falseSessionId = "AnotherId".ToLower();
        
        _sessionStorageMock.Setup(m => m.RunningSessions).Returns(runningSessions);
        
        OperationResult operationResult = _sessionService.PauseSession(falseSessionId);
     
        Assert.False(operationResult.Success);
    }
    
    #endregion
    
    #region ResumeSession

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ResumeSession_should_ResumePausedSession(
        bool sessionIdSpecified)
    {
        Session session = SessionProvider.CreateRandom();
        Dictionary<string, Session> pausedSessions = CreateSessionsDictionary(session);
        string sessionId = sessionIdSpecified ? session.Id : string.Empty;

        _sessionStorageMock.Setup(m => m.PausedSessions).Returns(pausedSessions);
        _sessionStorageMock.Setup(m => m.RemoveSession(SessionStatus.Paused, session.Id)).Returns(session);
        _sessionTimerMock.Setup(m => m.Start(session, It.IsAny<CancellationToken>()));

        OperationResult operationResult = _sessionService.ResumeSession(sessionId);
        
        Assert.True(operationResult.Success);
    }

    [Fact]
    public void ResumeSession_should_ReturnFailedOperation_when_ThereAreNoSessionsToResume()
    {
        _sessionStorageMock.Setup(m => m.PausedSessions)
            .Returns(new Dictionary<string, Session>());

        OperationResult operationResult = _sessionService.ResumeSession();
        
        Assert.False(operationResult.Success, operationResult.Message);
    }
    
    [Fact]
    public void ResumeSession_should_ReturnFailedOperation_when_IdNotFound()
    {
        Session session = SessionProvider.CreateRandom();
        Dictionary<string, Session> pausedSessions = CreateSessionsDictionary(session);
        string falseSessionId = "AnotherId".ToLower();
        
        _sessionStorageMock.Setup(m => m.PausedSessions).Returns(pausedSessions);

        OperationResult operationResult = _sessionService.ResumeSession(falseSessionId);
        
        Assert.False(operationResult.Success);
    }
    
    #endregion
    
    #region CancelSession
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CancelSession_should_CancelSession_when_SessionIsRunning(
        bool sessionIdSpecified)
    {
        // Arrange
        Session session = SessionProvider.CreateRandom();
        string sessionId = sessionIdSpecified ? session.Id : string.Empty;
        session.Status = SessionStatus.Executing;
        Dictionary<string, Session> runningSessions = CreateSessionsDictionary(session);
        
        _sessionTimerMock.Setup(m => m.Stop(session.Id)).Returns(session);
        _sessionStorageMock.Setup(m => m.RunningSessions).Returns(runningSessions);
        _sessionStorageMock.Setup(m => m.PausedSessions).Returns(new Dictionary<string, Session>());
        _sessionStorageMock.Setup(m => m.AddSession(SessionStatus.Cancelled, session)).Returns(true);

        // Act
        OperationResult operationResult = _sessionService.CancelSession(sessionId);
        
        // Assert
        Assert.True(operationResult.Success);
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CancelSession_should_CancelSession_when_SessionIsPaused(
        bool sessionIdSpecified)
    {
        // Arrange
        Session session = SessionProvider.CreateRandom();
        string sessionId = sessionIdSpecified ? session.Id : string.Empty;
        session.Status = SessionStatus.Paused;
        Dictionary<string, Session> pausedSessions = CreateSessionsDictionary(session);
        
        _sessionStorageMock.Setup(m => m.RemoveSession(SessionStatus.Paused, session.Id)).Returns(session);
        _sessionStorageMock.Setup(m => m.RunningSessions).Returns(new Dictionary<string, Session>());
        _sessionStorageMock.Setup(m => m.PausedSessions).Returns(pausedSessions);
        _sessionStorageMock.Setup(m => m.AddSession(SessionStatus.Cancelled, session)).Returns(true);

        // Act
        OperationResult operationResult = _sessionService.CancelSession(sessionId);
        
        // Assert
        Assert.True(operationResult.Success);
    }
    
    [Fact]
    public void CancelSession_should_ReturnFailedOperation_when_ThereAreNoSessionsToCancel()
    {
        Dictionary<string, Session> emptyDictionary = CreateSessionsDictionary();

        _sessionStorageMock.Setup(m => m.RunningSessions).Returns(emptyDictionary);
        _sessionStorageMock.Setup(m => m.PausedSessions).Returns(emptyDictionary);
        
        OperationResult operationResult = _sessionService.CancelSession();
        
        Assert.False(operationResult.Success);
    }
    
    [Fact]
    public void CancelSession_should_ReturnFailedOperation_when_IdNotFound()
    {
        Session session = SessionProvider.CreateRandom("FooSession");
        Dictionary<string, Session> runningSessions = CreateSessionsDictionary(session);
        Dictionary<string, Session> pausedSessions = CreateSessionsDictionary();
        string falseSessionId = "AnotherId".ToLower();

        _sessionStorageMock.Setup(m => m.RunningSessions).Returns(runningSessions);
        _sessionStorageMock.Setup(m => m.PausedSessions).Returns(pausedSessions);
        
        OperationResult operationResult = _sessionService.CancelSession(falseSessionId);
        
        Assert.False(operationResult.Success);
    }
    
    #endregion
    
    #region Events Handlers

    [Fact]
    public async Task SessionManager_should_ReportAndNotify_when_SessionIsStarted()
    {
        _notificationManagerMock.Setup(m => m.CloseLastNotificationAsync())
            .Returns(Task.CompletedTask).Verifiable(Times.Once);
        _notificationManagerMock.Setup(m => m.NotifyAsync(
            _notificationConfig.SessionStartedSummary,
            _notificationConfig.SessionStartedBody,
            NotificationSoundType.SessionStarted)).Returns(Task.CompletedTask).Verifiable(Times.Once);
        bool startServiceResult = await _sessionService.StartServiceAsync();
        
        await _sessionTimerMock.RaiseAsync(m => m.OnSessionStarted += null,
            _sessionTimerMock.Object, EventArgs.Empty);
        
        Assert.True(startServiceResult);
    }
    
    [Fact]
    public async Task SessionManager_should_ReportAndNotify_when_SessionIsCompleted()
    {
        Session sessionEventArg = SessionProvider.CreateRandom();
        
        _sessionStorageMock.Setup(m => m.AddSession(SessionStatus.Finished, sessionEventArg)).Returns(true);
        _stdOutQueueMock.Setup(m => m.QueueMessage(It.IsAny<string>()))
            .Verifiable(Times.Once);
        _notificationManagerMock.Setup(m => m.CloseLastNotificationAsync())
            .Returns(Task.CompletedTask).Verifiable(Times.Once);
        _notificationManagerMock.Setup(m => m.NotifyAsync(
            _notificationConfig.SessionFinishedSummary,
            _notificationConfig.SessionFinishedBody,
            NotificationSoundType.SessionFinished)).Returns(Task.CompletedTask).Verifiable(Times.Once);
        bool startServiceResult = await _sessionService.StartServiceAsync();
        
        await _sessionTimerMock.RaiseAsync(m => m.OnSessionCompleted += null,
            _sessionTimerMock.Object, sessionEventArg);
        
        Assert.True(startServiceResult);
    }

    // [Fact] TODO: Test fails when all solution tests are executed together.
    // public void SessionManager_should_SendMessagesToStdOut_when_SessionIsRunning()
    // {
    //     Session sessionReport = SessionProvider.CreateRandom();
    //
    //     _stdOutQueueMock.Setup(m => m.QueueMessage(It.IsAny<string>())).Verifiable(Times.Once);
    //     _sessionService.StartServiceAsync();
    //
    //     IProgress<Session> progress = _progress;
    //     progress.Report(sessionReport);
    // }

    [Theory]
    [InlineData(TimeType.Focus)]
    [InlineData(TimeType.Break)]
    public async Task SessionManager_should_ReportAndNotify_when_TimeIsCompleted(
        TimeType timeTypeCompleted)
    {
        // Arrange
        string summary = timeTypeCompleted is TimeType.Focus 
            ? _notificationConfig.FocusCompletedSummary : _notificationConfig.BreakCompletedSummary;
        string body = timeTypeCompleted is TimeType.Focus 
            ? _notificationConfig.FocusCompletedBody : _notificationConfig.BreakCompletedBody;
        
        _stdOutQueueMock.Setup(m => m.QueueMessage(It.IsAny<string>()))
            .Verifiable(Times.Once);
        _notificationManagerMock.Setup(m => m.CloseLastNotificationAsync())
            .Returns(Task.CompletedTask).Verifiable(Times.Once);
        _notificationManagerMock.Setup(m => m.NotifyAsync(
            summary, body, NotificationSoundType.TimeCompleted))
            .Returns(Task.CompletedTask).Verifiable(Times.Once);
        bool startServiceResult = await _sessionService.StartServiceAsync();
        
        // Act
        await _sessionTimerMock.RaiseAsync(m => m.OnTimeCompleted += null,
            _sessionTimerMock.Object, timeTypeCompleted);
        
        // Arrange
        Assert.True(startServiceResult);
    }
    
    [Fact]
    public async Task SessionManager_should_SendMessagesToStdOut_when_OnSessionDelayElapsed()
    {
        _stdOutQueueMock.Setup(m => m.QueueMessage(It.IsAny<string>()))
            .Verifiable(Times.Once);
        bool startServiceResult = await _sessionService.StartServiceAsync();

        await _sessionTimerMock.RaiseAsync(m => m.OnDelayElapsed += null,
            _sessionTimerMock.Object, TimeSpan.Zero);
        
        Assert.True(startServiceResult);
    }
    
    #endregion
    
    #region Helpers
    
    /// <summary>
    /// Generates test data for parameterized unit tests involving sessions and configurations.
    /// </summary>
    /// <param name="specifySessionId">Determines whether the session ID is explicitly provided or derived from the configuration ID.</param>
    /// <param name="specifyConfigId">Indicates whether the configuration ID is used to fetch the session configuration (passed through to test cases).</param>
    /// <returns>A <see cref="TheoryData{SessionConfig, Session, boolean, boolean}"/> containing test data for xUnit theories.</returns>
    public static TheoryData<SessionConfig, Session, bool, bool> GetSessionWithRandomConfig(bool specifySessionId, bool specifyConfigId)
    {
        var data = new TheoryData<SessionConfig, Session, bool, bool>();
    
        for (var i = 0; i < 5; i++)
        {
            SessionConfig config = SessionProvider.CreateRandomConfig($"Config_{i}");
            Session session = SessionProvider.Create(specifySessionId ? $"Session_{i}" : config.Id, config);
            
            data.Add(config, session, specifySessionId, specifyConfigId);
        }
    
        return data;
    }
    
    /// <summary>
    /// Converts a collection of sessions into a dictionary keyed by session ID.
    /// </summary>
    /// <param name="sessions">One or more sessions to include in the dictionary.</param>
    /// <returns>A dictionary where keys are session IDs and values are session objects.</returns>
    private static Dictionary<string, Session> CreateSessionsDictionary(params Session[] sessions)
    {
        return sessions.ToDictionary(x => x.Id, x => x);
    }
    
    #endregion
}
