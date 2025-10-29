using Microsoft.Extensions.Hosting;
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
    private readonly Mock<IStandardOutQueue> _stdOutQueueMock;
    private readonly Mock<TimeProvider> _timeProviderMock;
    private readonly Mock<IHostApplicationLifetime> _hostApplicationLifetimeMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Dictionary<string, Session> _activeSessions = [];
    private readonly NotificationConfig _notificationConfig;

    public SessionServiceTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _mockRepository = new MockRepository(MockBehavior.Strict);
        
        Mock<ILogger<SessionService>> loggerMock = _mockRepository.Create<ILogger<SessionService>>();
        _sessionConfigServiceMock = _mockRepository.Create<ISessionConfigService>();
        Mock<IOptions<NotificationConfig>> notificationOptionsMock = _mockRepository.Create<IOptions<NotificationConfig>>();
        _notificationManagerMock = _mockRepository.Create<INotificationService>(MockBehavior.Loose);
        _stdOutQueueMock = _mockRepository.Create<IStandardOutQueue>();
        _timeProviderMock = _mockRepository.Create<TimeProvider>(MockBehavior.Loose);
        _hostApplicationLifetimeMock = _mockRepository.Create<IHostApplicationLifetime>();
        _loggerFactoryMock = _mockRepository.Create<ILoggerFactory>(MockBehavior.Loose);

        _notificationConfig = new NotificationConfig();
        notificationOptionsMock.Setup(n => n.Value).Returns(_notificationConfig);
        
        _sessionService = new SessionService(
            loggerMock.Object,
            _sessionConfigServiceMock.Object,
            notificationOptionsMock.Object,
            _notificationManagerMock.Object,
            _stdOutQueueMock.Object,
            _timeProviderMock.Object,
            _hostApplicationLifetimeMock.Object,
            _loggerFactoryMock.Object,
            _activeSessions);
    }

    public void Dispose()
    {
        // Global Arrange
        _mockRepository.VerifyAll();

        foreach (var session in _activeSessions)
        {
            try
            {
                session.Value.Dispose();
            }
            catch (Exception ex)
            {
                // Ignore temporarily.
            }
        }

        _activeSessions.Clear();
    }
    
    #region StartSession
    
    [Theory]
    [MemberData(nameof(GetSessionWithRandomConfig), true, true)]
    [MemberData(nameof(GetSessionWithRandomConfig), true, false)]
    [MemberData(nameof(GetSessionWithRandomConfig), false, true)]
    [MemberData(nameof(GetSessionWithRandomConfig), false, false)]
    public async Task StartSession_should_StartSession(
        SessionConfig config, Session session, bool specifySessionId, bool specifyConfigId)
    {
        // Arrange
        _hostApplicationLifetimeMock.Setup(m => m.ApplicationStopping).Returns(It.IsAny<CancellationToken>());
        if (specifyConfigId)
        {
            _sessionConfigServiceMock.Setup(m => m.TryGetConfigById(config.Id, out config))
                .Returns(true);
        }
        else
            _sessionConfigServiceMock.Setup(m => m.DefaultConfig).Returns(config);

        // Act
        OperationResult operationResult = specifySessionId switch
        {
            // Both IDs specified.
            true when specifyConfigId => await _sessionService.StartSessionAsync(session.Id, config.Id),
            // Only session ID specified.
            true when !specifyConfigId => await _sessionService.StartSessionAsync(session.Id),
            // Only config ID specified.
            false when specifyConfigId => await _sessionService.StartSessionAsync(sessionConfigId: config.Id),
            _ => await _sessionService.StartSessionAsync()
        };
        
        // Assert
        Assert.True(operationResult.Success);
    }

    [Fact]
    public async Task StartSession_should_ReturnFailedResult_when_ConfigIdNotExists()
    {
        SessionConfig config = SessionProvider.CreateRandomConfig();
        
        _sessionConfigServiceMock.Setup(m => m.TryGetConfigById(It.IsAny<string>(), out config))
            .Returns(false);
        
        OperationResult operationResult = await _sessionService.StartSessionAsync(sessionConfigId: config.Id);
        
        Assert.False(operationResult.Success);
    }
    
    [Fact]
    public async Task StartSession_should_ReturnFailedResult_when_SessionIdAlreadyExists()
    {
        SessionConfig config = SessionProvider.CreateConfig();
        Session session = SessionProvider.CreateRandom();
        _sessionConfigServiceMock.Setup(m => m.DefaultConfig).Returns(config);
        _activeSessions.Add(session.Id, session);
        
        OperationResult operationResult = await _sessionService.StartSessionAsync(session.Id);
        
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
        Session session = SessionProvider.CreateRandom();
        string sessionId = specifySessionId ? session.Id : string.Empty;
        session.Start();
        _activeSessions.Add(sessionId, session);

        OperationResult operationResult = _sessionService.PauseSession(sessionId);
     
        Assert.True(operationResult.Success);
    }
    
    [Fact]
    public void PauseSession_should_ReturnErrorResult_when_ThereAreNoSessionsToPause()
    {
        OperationResult operationResult = _sessionService.PauseSession();
     
        Assert.False(operationResult.Success);
    }
    
    [Fact]
    public void PauseSession_should_ReturnErrorResult_when_SessionIdNotFound()
    {
        string falseSessionId = "AnotherId".ToLower();
        
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
        string sessionId = sessionIdSpecified ? session.Id : string.Empty;
        session.Pause();
        _activeSessions.Add(sessionId, session);

        OperationResult operationResult = _sessionService.ResumeSession(sessionId);
        
        Assert.True(operationResult.Success);
    }

    [Fact]
    public void ResumeSession_should_ReturnFailedOperation_when_ThereAreNoSessionsToResume()
    {
        OperationResult operationResult = _sessionService.ResumeSession();
        
        Assert.False(operationResult.Success, operationResult.Message);
    }
    
    [Fact]
    public void ResumeSession_should_ReturnFailedOperation_when_IdNotFound()
    {
        string falseSessionId = "AnotherId".ToLower();

        OperationResult operationResult = _sessionService.ResumeSession(falseSessionId);
        
        Assert.False(operationResult.Success);
    }
    
    #endregion
    
    #region CancelSession
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CancelSession_should_CancelSession_when_SessionIsRunning(
        bool sessionIdSpecified)
    {
        // Arrange
        Session session = SessionProvider.CreateRandom();
        string sessionId = sessionIdSpecified ? session.Id : string.Empty;
        session.Start();
        _activeSessions.Add(sessionId, session);

        // Act
        OperationResult operationResult = await _sessionService.CancelSessionAsync(sessionId);
        
        // Assert
        Assert.True(operationResult.Success);
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CancelSession_should_CancelSession_when_SessionIsPaused(
        bool sessionIdSpecified)
    {
        // Arrange
        Session session = SessionProvider.CreateRandom();
        string sessionId = sessionIdSpecified ? session.Id : string.Empty;
        session.Pause();
        _activeSessions.Add(sessionId, session);

        // Act
        OperationResult operationResult = await _sessionService.CancelSessionAsync(sessionId);
        
        // Assert
        Assert.True(operationResult.Success);
    }
    
    [Fact]
    public async Task CancelSession_should_ReturnFailedOperation_when_ThereAreNoSessionsToCancel()
    {
        OperationResult operationResult = await _sessionService.CancelSessionAsync();
        Assert.False(operationResult.Success);
    }
    
    [Fact]
    public async Task CancelSession_should_ReturnFailedOperation_when_IdNotFound()
    {
        string falseSessionId = "AnotherId".ToLower();
        
        OperationResult operationResult = await _sessionService.CancelSessionAsync(falseSessionId);
        
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
