using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

using Tiempito.Daemon.Application.Config.Sessions;
using Tiempito.Daemon.Application.Notifications;
using Tiempito.Daemon.Application.Sessions;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Sessions;
using Tiempito.Daemon.Domain.Sessions.Enums;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.Daemon.Server;
using Tiempito.Daemon.Server.Configuration;
using Tiempito.Daemon.Tests.Sessions.Helpers;

using Xunit.Abstractions;

namespace Tiempito.Daemon.Tests.Sessions;

[Trait("Sessions", "Unit")]
public class SessionServiceTests : IDisposable
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly SessionService _sessionService;
    private readonly MockRepository _mockRepository;
    private readonly Mock<ISessionConfigService> _sessionConfigServiceMock;
    private readonly Mock<INotificationService> _notificationManagerMock;
    private readonly Mock<ISessionStorage> _sessionStorageMock;
    private readonly Mock<IStandardOutQueue> _stdOutQueueMock;
    private readonly Mock<TimeProvider> _timeProviderMock;
    private readonly NotificationConfig _notificationConfig;

    public SessionServiceTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _mockRepository = new MockRepository(MockBehavior.Strict);
        
        Mock<ILogger<SessionService>> loggerMock = _mockRepository.Create<ILogger<SessionService>>();
        _sessionConfigServiceMock = _mockRepository.Create<ISessionConfigService>();
        Mock<IOptions<NotificationConfig>> notificationOptionsMock = _mockRepository.Create<IOptions<NotificationConfig>>();
        _notificationManagerMock = _mockRepository.Create<INotificationService>(MockBehavior.Loose);
        _sessionStorageMock = _mockRepository.Create<ISessionStorage>();
        _stdOutQueueMock = _mockRepository.Create<IStandardOutQueue>();
        _timeProviderMock = _mockRepository.Create<TimeProvider>(MockBehavior.Loose);

        _notificationConfig = new NotificationConfig();
        notificationOptionsMock.Setup(n => n.Value).Returns(_notificationConfig);
        
        _sessionService = new SessionService(
            loggerMock.Object,
            _sessionConfigServiceMock.Object,
            notificationOptionsMock.Object,
            _notificationManagerMock.Object,
            _sessionStorageMock.Object,
            _stdOutQueueMock.Object,
            _timeProviderMock.Object);
    }

    public void Dispose()
    {
        // Global Arrange
        _mockRepository.VerifyAll();
    }
    
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
        _sessionStorageMock.Setup(m => m.RunningSessions).Returns(new Dictionary<string, Session>());
        _sessionStorageMock.Setup(m => m.PausedSessions).Returns(new Dictionary<string, Session>());
        _sessionStorageMock.Setup(m => m.AddSession(SessionStatus.Executing, It.IsAny<Session>())).Returns(true);

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
        _sessionStorageMock.Setup(m => m.RemoveSession(SessionStatus.Executing, session.Id)).Returns(session);
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
        _sessionStorageMock.Setup(m => m.AddSession(SessionStatus.Executing, session)).Returns(true);

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
        session.Start();
        Dictionary<string, Session> runningSessions = CreateSessionsDictionary(session);
        
        _sessionStorageMock.Setup(m => m.RemoveSession(SessionStatus.Executing, session.Id)).Returns(session);
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
        session.Start();
        session.Pause();
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
