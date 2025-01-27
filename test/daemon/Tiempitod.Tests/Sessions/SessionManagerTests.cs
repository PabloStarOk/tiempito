using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Runtime.InteropServices;
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
    private readonly Mock<ILogger<SessionManager>> _loggerMock;
    private readonly Mock<ISessionConfigProvider> _sessionConfigProviderMock;
    private readonly Mock<IOptions<NotificationConfig>> _notificationOptionsMock;
    private readonly Mock<INotificationManager> _notificationManagerMock;
    private readonly Progress<Session> _progress;
    private readonly Mock<ISessionStorage> _sessionStorageMock;
    private readonly Mock<ISessionTimer> _sessionTimerMock;
    private readonly Mock<IStandardOutQueue> _stdOutQueueMock;
    private readonly NotificationConfig _notificationConfig;
    
    public SessionManagerTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Strict);
        
        _loggerMock = _mockRepository.Create<ILogger<SessionManager>>();
        _sessionConfigProviderMock = _mockRepository.Create<ISessionConfigProvider>();
        _notificationOptionsMock = _mockRepository.Create<IOptions<NotificationConfig>>();
        _notificationManagerMock = _mockRepository.Create<INotificationManager>();
        _progress = new Progress<Session>();
        _sessionStorageMock = _mockRepository.Create<ISessionStorage>();
        _sessionTimerMock = _mockRepository.Create<ISessionTimer>();
        _stdOutQueueMock = _mockRepository.Create<IStandardOutQueue>();

        _notificationConfig = new NotificationConfig();
        _notificationOptionsMock.Setup(n => n.Value).Returns(_notificationConfig);
        
        _sessionManager = new SessionManager(
            _loggerMock.Object,
            _sessionConfigProviderMock.Object,
            _notificationOptionsMock.Object,
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
        Dictionary<string, Session> runningSessionsDict = CreateSessionsDictionary(session);
        
        _sessionConfigProviderMock.Setup(m => m.DefaultSessionConfig).Returns(config);
        _sessionStorageMock.Setup(m => m.RunningSessions).Returns(runningSessionsDict);
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
        bool specifyId)
    {
        Session session = SessionProvider.Create();
        Dictionary<string, Session> runningSessionsDict = CreateSessionsDictionary(session);
        
        _sessionStorageMock.Setup(m => m.RunningSessions).Returns(runningSessionsDict);
        _sessionTimerMock.Setup(m => m.Stop(session.Id)).Returns(session);
        _sessionStorageMock.Setup(m => m.AddSession(SessionStatus.Paused, session)).Returns(true);
        
        OperationResult operationResult = _sessionManager.PauseSession(specifyId ? session.Id : "");
     
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
        string anotherId = "AnotherId".ToLower();
        Session session = SessionProvider.Create();
        Dictionary<string, Session> runningSessionsDict = CreateSessionsDictionary(session);
        
        _sessionStorageMock.Setup(m => m.RunningSessions).Returns(runningSessionsDict);
        
        OperationResult operationResult = _sessionManager.PauseSession(anotherId);
     
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
    public void ResumeSession_should_ReturnFailedOperation_when_IdNotFound()
    {
        Session session = SessionProvider.CreateRandom();
        Dictionary<string, Session> pausedSessions = CreateSessionsDictionary(session);
        var falseSessionId = "AnotherId";
        
        _sessionStorageMock.Setup(m => m.PausedSessions).Returns(pausedSessions);

        OperationResult operationResult = _sessionManager.ResumeSession(falseSessionId);
        
        Assert.False(operationResult.Success);
    }

    [Fact]
    public void ResumeSession_should_ReturnFailedOperation_when_ThereAreNoSessions()
    {
        _sessionStorageMock.Setup(m => m.PausedSessions)
            .Returns(new Dictionary<string, Session>());

        OperationResult operationResult = _sessionManager.ResumeSession();
        
        Assert.False(operationResult.Success, operationResult.Message);
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
