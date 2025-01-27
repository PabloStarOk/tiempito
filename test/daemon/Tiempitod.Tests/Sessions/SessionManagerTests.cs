using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Tiempitod.NET;
using Tiempitod.NET.Configuration.Notifications;
using Tiempitod.NET.Configuration.Session;
using Tiempitod.NET.Notifications;
using Tiempitod.NET.Server;
using Tiempitod.NET.Session;
using Tiempitod.Tests.Sessions.Helpers;

namespace Tiempitod.Tests.Sessions;

[Trait("Sessions", "Unit")]
public class SessionManagerTests : IDisposable
{
    private readonly SessionManager _sessionManager;
    private readonly MockRepository _mockRepository;
    private readonly Mock<ISessionConfigProvider> _sessionConfigProviderMock;
    private readonly Mock<INotificationManager> _notificationManagerMock;
    private readonly Progress<Session> _progress;
    private readonly Mock<ISessionStorage> _sessionStorageMock;
    private readonly Mock<ISessionTimer> _sessionTimerMock;
    private readonly Mock<IStandardOutQueue> _stdOutQueueMock;
    private readonly NotificationConfig _notificationConfig;
    
    public SessionManagerTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Strict);
        
        Mock<ILogger<SessionManager>> loggerMock = _mockRepository.Create<ILogger<SessionManager>>();
        _sessionConfigProviderMock = _mockRepository.Create<ISessionConfigProvider>();
        Mock<IOptions<NotificationConfig>> notificationOptionsMock = _mockRepository.Create<IOptions<NotificationConfig>>();
        _notificationManagerMock = _mockRepository.Create<INotificationManager>();
        _progress = new Progress<Session>();
        _sessionStorageMock = _mockRepository.Create<ISessionStorage>();
        _sessionTimerMock = _mockRepository.Create<ISessionTimer>();
        _stdOutQueueMock = _mockRepository.Create<IStandardOutQueue>();

        _notificationConfig = new NotificationConfig();
        notificationOptionsMock.Setup(n => n.Value).Returns(_notificationConfig);
        
        _sessionManager = new SessionManager(
            loggerMock.Object,
            _sessionConfigProviderMock.Object,
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
        _mockRepository.VerifyAll();
    }
    
    #region DaemonService Methods

    [Fact]
    public void StartService_should_SubscribeToEvents()
    {
        _sessionManager.StartService();
        _sessionTimerMock.VerifyAdd(m => m.OnTimeCompleted += It.IsAny<EventHandler<TimeType>>(), Times.Once);
        _sessionTimerMock.VerifyAdd(m => m.OnDelayElapsed += It.IsAny<EventHandler<TimeSpan>>(), Times.Once);
        _sessionTimerMock.VerifyAdd(m => m.OnSessionStarted += It.IsAny<EventHandler>(), Times.Once);
        _sessionTimerMock.VerifyAdd(m => m.OnSessionCompleted += It.IsAny<EventHandler<Session>>(), Times.Once);
    }
    
    [Fact]
    public void StopService_should_UnsubscribeFromEventsAndStopTimers()
    {
        _sessionTimerMock.Setup(m => m.StopAll()).Returns([]);
        
        _sessionManager.StopService();
        
        _sessionTimerMock.VerifyAdd(m => m.OnTimeCompleted += It.IsAny<EventHandler<TimeType>>(), Times.Never);
        _sessionTimerMock.VerifyAdd(m => m.OnDelayElapsed += It.IsAny<EventHandler<TimeSpan>>(), Times.Never);
        _sessionTimerMock.VerifyAdd(m => m.OnSessionStarted += It.IsAny<EventHandler>(), Times.Never);
        _sessionTimerMock.VerifyAdd(m => m.OnSessionCompleted += It.IsAny<EventHandler<Session>>(), Times.Never);
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
            _sessionConfigProviderMock.Setup(m => m.SessionConfigs.TryGetValue(config.Id, out config))
                .Returns(true);
        }
        else
            _sessionConfigProviderMock.Setup(m => m.DefaultSessionConfig).Returns(config);
        _sessionTimerMock.Setup(m => m.Start(session, It.IsAny<CancellationToken>()));
        _sessionStorageMock.Setup(m => m.RunningSessions).Returns(new Dictionary<string, Session>());
        _sessionStorageMock.Setup(m => m.PausedSessions).Returns(new Dictionary<string, Session>());

        // Act
        OperationResult operationResult = specifySessionId switch
        {
            // Both IDs specified.
            true when specifyConfigId => _sessionManager.StartSession(session.Id, config.Id),
            // Only session ID specified.
            true when !specifyConfigId => _sessionManager.StartSession(session.Id),
            // Only config ID specified.
            false when specifyConfigId => _sessionManager.StartSession(sessionConfigId: config.Id),
            _ => _sessionManager.StartSession()
        };
        
        // Assert
        Assert.True(operationResult.Success);
    }

    [Fact]
    public void StartSession_should_ReturnFailedResult_when_ConfigIdNotExists()
    {
        SessionConfig config = SessionProvider.CreateRandomConfig();
        
        _sessionConfigProviderMock.Setup(m => m.SessionConfigs.TryGetValue(It.IsAny<string>(), out config))
            .Returns(false);
        
        OperationResult operationResult = _sessionManager.StartSession(sessionConfigId: config.Id);
        
        Assert.False(operationResult.Success);
    }
    
    [Fact]
    public void StartSession_should_ReturnFailedResult_when_SessionIdAlreadyExists()
    {
        SessionConfig config = SessionProvider.CreateConfig();
        Session session = SessionProvider.CreateRandom();
        Dictionary<string, Session> runningSessions = CreateSessionsDictionary(session);
        
        _sessionConfigProviderMock.Setup(m => m.DefaultSessionConfig).Returns(config);
        _sessionStorageMock.Setup(m => m.RunningSessions).Returns(runningSessions);
        _sessionStorageMock.Setup(m => m.PausedSessions).Returns(new Dictionary<string, Session>());
        
        OperationResult operationResult = _sessionManager.StartSession(session.Id);
        
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
        
        OperationResult operationResult = _sessionManager.PauseSession(sessionId);
     
        Assert.True(operationResult.Success);
    }
    
    [Fact]
    public void PauseSession_should_ReturnErrorResult_when_ThereAreNoSessionsToPause()
    {
        _sessionStorageMock.Setup(m => m.RunningSessions).Returns(new Dictionary<string, Session>());
        
        OperationResult operationResult = _sessionManager.PauseSession();
     
        Assert.False(operationResult.Success);
    }
    
    [Fact]
    public void PauseSession_should_ReturnErrorResult_when_SessionIdNotFound()
    {
        Session session = SessionProvider.Create();
        Dictionary<string, Session> runningSessions = CreateSessionsDictionary(session);
        string falseSessionId = "AnotherId".ToLower();
        
        _sessionStorageMock.Setup(m => m.RunningSessions).Returns(runningSessions);
        
        OperationResult operationResult = _sessionManager.PauseSession(falseSessionId);
     
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

        OperationResult operationResult = _sessionManager.ResumeSession(sessionId);
        
        Assert.True(operationResult.Success);
    }

    [Fact]
    public void ResumeSession_should_ReturnFailedOperation_when_ThereAreNoSessionsToResume()
    {
        _sessionStorageMock.Setup(m => m.PausedSessions)
            .Returns(new Dictionary<string, Session>());

        OperationResult operationResult = _sessionManager.ResumeSession();
        
        Assert.False(operationResult.Success, operationResult.Message);
    }
    
    [Fact]
    public void ResumeSession_should_ReturnFailedOperation_when_IdNotFound()
    {
        Session session = SessionProvider.CreateRandom();
        Dictionary<string, Session> pausedSessions = CreateSessionsDictionary(session);
        string falseSessionId = "AnotherId".ToLower();
        
        _sessionStorageMock.Setup(m => m.PausedSessions).Returns(pausedSessions);

        OperationResult operationResult = _sessionManager.ResumeSession(falseSessionId);
        
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
        OperationResult operationResult = _sessionManager.CancelSession(sessionId);
        
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
        OperationResult operationResult = _sessionManager.CancelSession(sessionId);
        
        // Assert
        Assert.True(operationResult.Success);
    }
    
    [Fact]
    public void CancelSession_should_ReturnFailedOperation_when_ThereAreNoSessionsToCancel()
    {
        Dictionary<string, Session> emptyDictionary = CreateSessionsDictionary();

        _sessionStorageMock.Setup(m => m.RunningSessions).Returns(emptyDictionary);
        _sessionStorageMock.Setup(m => m.PausedSessions).Returns(emptyDictionary);
        
        OperationResult operationResult = _sessionManager.CancelSession();
        
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
        
        OperationResult operationResult = _sessionManager.CancelSession(falseSessionId);
        
        Assert.False(operationResult.Success);
    }
    
    #endregion
    
    #region Events Handlers

    [Fact]
    public void SessionManager_should_ReportAndNotify_when_SessionIsStarted()
    {
        _notificationManagerMock.Setup(m => m.CloseLastNotificationAsync()).Returns(Task.CompletedTask);
        _notificationManagerMock.Setup(m => m.NotifyAsync(
            _notificationConfig.SessionStartedSummary,
            _notificationConfig.SessionStartedBody,
            NotificationSoundType.SessionStarted)).Returns(Task.CompletedTask);
        _sessionManager.StartService();
        
        _sessionTimerMock.Raise(m => m.OnSessionStarted += null,
            _sessionTimerMock.Object, EventArgs.Empty);
    }
    
    [Fact]
    public void SessionManager_should_ReportAndNotify_when_SessionIsCompleted()
    {
        Session sessionEventArg = SessionProvider.CreateRandom();
        
        _sessionStorageMock.Setup(m => m.AddSession(SessionStatus.Finished, sessionEventArg)).Returns(true);
        _stdOutQueueMock.Setup(m => m.QueueMessage(It.IsAny<string>()));
        _notificationManagerMock.Setup(m => m.CloseLastNotificationAsync()).Returns(Task.CompletedTask);
        _notificationManagerMock.Setup(m => m.NotifyAsync(
            _notificationConfig.SessionFinishedSummary,
            _notificationConfig.SessionFinishedBody,
            NotificationSoundType.SessionFinished)).Returns(Task.CompletedTask);
        _sessionManager.StartService();
        
        _sessionTimerMock.Raise(m => m.OnSessionCompleted += null,
            _sessionTimerMock.Object, sessionEventArg);
    }

    // [Fact] TODO: Test fails when all solution tests are executed together.
    // public void SessionManager_should_SendMessagesToStdOut_when_SessionIsRunning()
    // {
    //     Session sessionReport = SessionProvider.CreateRandom();
    //
    //     _stdOutQueueMock.Setup(m => m.QueueMessage(It.IsAny<string>())).Verifiable(Times.Once);
    //     _sessionManager.StartService();
    //
    //     IProgress<Session> progress = _progress;
    //     progress.Report(sessionReport);
    // }

    [Theory]
    [InlineData(TimeType.Focus)]
    [InlineData(TimeType.Break)]
    public void SessionManager_should_ReportAndNotify_when_TimeIsCompleted(
        TimeType timeTypeCompleted)
    {
        // Arrange
        string summary = timeTypeCompleted is TimeType.Focus 
            ? _notificationConfig.FocusCompletedSummary : _notificationConfig.BreakCompletedSummary;
        string body = timeTypeCompleted is TimeType.Focus 
            ? _notificationConfig.FocusCompletedBody : _notificationConfig.BreakCompletedBody;
        
        _stdOutQueueMock.Setup(m => m.QueueMessage(It.IsAny<string>()));
        _notificationManagerMock.Setup(m => m.CloseLastNotificationAsync()).Returns(Task.CompletedTask);
        _notificationManagerMock.Setup(m => m.NotifyAsync(
            summary, body, NotificationSoundType.TimeCompleted)).Returns(Task.CompletedTask);
        _sessionManager.StartService();
        
        // Act
        _sessionTimerMock.Raise(m => m.OnTimeCompleted += null,
            _sessionTimerMock.Object, timeTypeCompleted);
    }
    
    [Fact]
    public void SessionManager_should_SendMessagesToStdOut_when_OnSessionDelayElapsed()
    {
        _stdOutQueueMock.Setup(m => m.QueueMessage(It.IsAny<string>()));
        _sessionManager.StartService();

        _sessionTimerMock.Raise(m => m.OnDelayElapsed += null,
            _sessionTimerMock.Object, TimeSpan.Zero);
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
