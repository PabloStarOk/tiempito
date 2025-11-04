using Microsoft.Extensions.Hosting;

using Moq;

using Tiempito.Daemon.Application.Notifications;
using Tiempito.Daemon.Application.Sessions;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Notifications.Enums;
using Tiempito.Daemon.Domain.Sessions;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.Daemon.Tests.Sessions.Helpers;

namespace Tiempito.Daemon.Tests.Application.Sessions;

/// <summary>
/// Unit tests for <see cref="SessionService"/> covering session lifecycle operations.
/// </summary>
[Trait("Sessions", "Unit")]
public class SessionServiceTests : IDisposable
{
    private readonly SessionService _sessionService;
    private readonly MockRepository _mockRepository;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IStandardOutQueueWriter> _stdOutQueueWriterMock;
    private readonly Mock<IHostApplicationLifetime> _appLifetimeMock;
    private readonly Mock<ISessionFactory> _sessionFactoryMock;
<<<<<<< HEAD
    private readonly Dictionary<string, ISession> _activeSessions = [];
=======
    private readonly Dictionary<string, Session> _fakeActiveSessions;
>>>>>>> aa55073 (test(daemon): update SessionService tests)

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionServiceTests"/> class,
    /// setting up all required mocks and the test instance of <see cref="SessionService"/>.
    /// </summary>
    public SessionServiceTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Strict);
        _notificationServiceMock = _mockRepository.Create<INotificationService>();
        _stdOutQueueWriterMock = _mockRepository.Create<IStandardOutQueueWriter>();
        _appLifetimeMock = _mockRepository.Create<IHostApplicationLifetime>();
        _sessionFactoryMock = _mockRepository.Create<ISessionFactory>();
        _fakeActiveSessions = new Dictionary<string, Session>();

        _sessionService = new SessionService(
            _notificationServiceMock.Object,
            _stdOutQueueWriterMock.Object,
            _appLifetimeMock.Object,
            _sessionFactoryMock.Object,
            _fakeActiveSessions);
    }

<<<<<<< HEAD
    /// <inheritdoc/>
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
            _sessionFactoryMock.Setup(m => m.ExistsConfig(config.Id)).Returns(true);
        }

        _sessionFactoryMock.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(session);

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
        
        _sessionFactoryMock.Setup(m => m.ExistsConfig(config.Id)).Returns(false);
        
        OperationResult operationResult = await _sessionService.StartSessionAsync(sessionConfigId: config.Id);
        
        Assert.False(operationResult.Success);
    }
    
    [Fact]
    public async Task StartSession_should_ReturnFailedResult_when_SessionIdAlreadyExists()
    {
        Session session = SessionProvider.CreateRandom();
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
    public void CancelSession_should_CancelSession_when_SessionIsRunning(
        bool sessionIdSpecified)
    {
        // Arrange
        Session session = SessionProvider.CreateRandom();
        string sessionId = sessionIdSpecified ? session.Id : string.Empty;
        session.Start();
        _activeSessions.Add(sessionId, session);

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
        session.Pause();
        _activeSessions.Add(sessionId, session);

        // Act
        OperationResult operationResult = _sessionService.CancelSession(sessionId);
        
        // Assert
        Assert.True(operationResult.Success);
    }
    
    [Fact]
    public void CancelSession_should_ReturnFailedOperation_when_ThereAreNoSessionsToCancel()
    {
        OperationResult operationResult = _sessionService.CancelSession();
        Assert.False(operationResult.Success);
    }
    
    [Fact]
    public void CancelSession_should_ReturnFailedOperation_when_IdNotFound()
    {
        string falseSessionId = "AnotherId".ToLower();
        
        OperationResult operationResult = _sessionService.CancelSession(falseSessionId);
        
        Assert.False(operationResult.Success);
    }
    
    #endregion
    
    #region Helpers
    
=======
>>>>>>> aa55073 (test(daemon): update SessionService tests)
    /// <summary>
    /// Generates test data for parameterized unit tests involving sessions and configurations.
    /// </summary>
    /// <param name="specifySessionId">Determines whether the session ID is explicitly provided or derived from the configuration ID.</param>
    /// <param name="specifyConfigId">Indicates whether the configuration ID is used to fetch the session configuration (passed through to test cases).</param>
    /// <returns>A <see cref="TheoryData{SessionConfig, Session, boolean, boolean}"/> containing test data for xUnit theories.</returns>
    public static TheoryData<SessionConfig, Session, bool, bool> GetSessionWithRandomConfig(
        bool specifySessionId,
        bool specifyConfigId)
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

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Tests that <see cref="SessionService.StartSessionAsync"/> successfully starts a session
    /// using various combinations of session and configuration IDs.
    /// </summary>
    /// <param name="config">The session configuration to use.</param>
    /// <param name="session">The session instance to start.</param>
    /// <param name="specifySessionId">Whether the session ID is explicitly provided.</param>
    /// <param name="specifyConfigId">Whether the configuration ID is explicitly provided.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [MemberData(nameof(GetSessionWithRandomConfig), true, true)]
    [MemberData(nameof(GetSessionWithRandomConfig), true, false)]
    [MemberData(nameof(GetSessionWithRandomConfig), false, true)]
    [MemberData(nameof(GetSessionWithRandomConfig), false, false)]
    public async Task StartSessionAsync_should_StartSession(
        SessionConfig config,
        Session session,
        bool specifySessionId,
        bool specifyConfigId)
    {
        // Arrange
        _appLifetimeMock.Setup(m => m.ApplicationStopping).Returns(It.IsAny<CancellationToken>());
        _notificationServiceMock.Setup(m => m.NotifyAsync(session.State, NotificationType.SessionStarted))
            .Returns(ValueTask.CompletedTask);
        if (specifyConfigId)
        {
            _sessionFactoryMock.Setup(m => m.ExistsConfig(config.Id)).Returns(true);
        }

        _sessionFactoryMock.Setup(m => m.Create(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Func<Session, ValueTask>>(),
                It.IsAny<Func<Session, ValueTask>>(),
                It.IsAny<Func<Session, ValueTask>>()))
            .Returns(session);
        string sessionId = specifySessionId ? session.Id : string.Empty;
        string configId = specifyConfigId ? config.Id : string.Empty;

        // Act
        OperationResult actual = await _sessionService.StartSessionAsync(sessionId, configId);

        // Assert
        Assert.True(actual.Success);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.StartSessionAsync"/> returns an error when the specified configuration ID is not found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StartSessionAsync_return_Error_when_ConfigIdNotFound()
    {
        SessionConfig config = SessionProvider.CreateRandomConfig();
        _sessionFactoryMock.Setup(m => m.ExistsConfig(config.Id)).Returns(false);

        OperationResult actual = await _sessionService.StartSessionAsync(string.Empty, config.Id);

        Assert.False(actual.Success);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.StartSessionAsync"/> returns an error when the session ID already exists in active sessions.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StartSessionAsync_return_Error_when_SessionIdAlreadyExists()
    {
        Session session = SessionProvider.CreateRandom();
        _fakeActiveSessions.Add(session.Id, session);

        OperationResult actual = await _sessionService.StartSessionAsync(session.Id);

        Assert.False(actual.Success);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.PauseSession"/> successfully pauses a session
    /// when the session ID is specified or not.
    /// </summary>
    /// <param name="specifySessionId">Indicates whether the session ID is explicitly provided.</param>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PauseSession_should_PauseSession(bool specifySessionId)
    {
        Session session = SessionProvider.CreateRandom();
        string sessionId = specifySessionId ? session.Id : string.Empty;
        session.Start();
        _fakeActiveSessions.Add(sessionId, session);

        OperationResult actual = _sessionService.PauseSession(sessionId);

        Assert.True(actual.Success);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.PauseSession"/> returns an error when there are no sessions to pause.
    /// </summary>
    [Fact]
    public void PauseSession_return_Error_when_NoSessionsToPause()
    {
        OperationResult actual = _sessionService.PauseSession();

        Assert.False(actual.Success);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.PauseSession"/> returns an error when the specified session ID is not found.
    /// </summary>
    [Fact]
    public void PauseSession_return_Error_when_SessionIdNotFound()
    {
        const string idStub = "AnotherId";

        OperationResult actual = _sessionService.PauseSession(idStub);

        Assert.False(actual.Success);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.ResumeSession"/> successfully resumes a paused session
    /// when the session ID is specified or not.
    /// </summary>
    /// <param name="specifySessionId">Indicates whether the session ID is explicitly provided.</param>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ResumeSession_should_ResumePausedSession(bool specifySessionId)
    {
        Session session = SessionProvider.CreateRandom();
        string sessionId = specifySessionId ? session.Id : string.Empty;
        session.Pause();
        _fakeActiveSessions.Add(sessionId, session);

        OperationResult actual = _sessionService.ResumeSession(sessionId);

        Assert.True(actual.Success);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.ResumeSession"/> returns an error when there are no sessions to resume.
    /// </summary>
    [Fact]
    public void ResumeSession_return_Error_when_NoSessionsToResume()
    {
        OperationResult actual = _sessionService.ResumeSession();

        Assert.False(actual.Success);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.ResumeSession"/> returns an error when the specified session ID is not found.
    /// </summary>
    [Fact]
    public void ResumeSession_return_Error_when_SessionIdNotFound()
    {
        const string idStub = "AnotherId";

        OperationResult actual = _sessionService.ResumeSession(idStub);

        Assert.False(actual.Success);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.CancelSessionAsync"/> successfully cancels a running session
    /// when the session ID is specified or not.
    /// </summary>
    /// <param name="specifySessionId">Indicates whether the session ID is explicitly provided.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CancelSessionAsync_should_CancelSession_when_SessionIsRunning(bool specifySessionId)
    {
        Session session = SessionProvider.CreateRandom();
        string sessionId = specifySessionId ? session.Id : string.Empty;
        session.Start();
        _fakeActiveSessions.Add(sessionId, session);

        OperationResult actual = await _sessionService.CancelSessionAsync(sessionId);

        Assert.True(actual.Success);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.CancelSessionAsync"/> successfully cancels a paused session
    /// when the session ID is specified or not.
    /// </summary>
    /// <param name="specifySessionId">Indicates whether the session ID is explicitly provided.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CancelSessionAsync_should_CancelSession_when_SessionIsPaused(bool specifySessionId)
    {
        Session session = SessionProvider.CreateRandom();
        string sessionId = specifySessionId ? session.Id : string.Empty;
        session.Pause();
        _fakeActiveSessions.Add(sessionId, session);

        OperationResult actual = await _sessionService.CancelSessionAsync(sessionId);

        Assert.True(actual.Success);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.CancelSessionAsync"/> returns an error when there are no sessions to cancel.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CancelSessionAsync_return_Error_when_NoSessionsToCancel()
    {
        OperationResult operationResult = await _sessionService.CancelSessionAsync();

        Assert.False(operationResult.Success);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.CancelSessionAsync"/> returns a failed operation
    /// when the specified session ID is not found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CancelSession_should_ReturnFailedOperation_when_IdNotFound()
    {
        const string stubId = "AnotherId";

        OperationResult actual = await _sessionService.CancelSessionAsync(stubId);

        Assert.False(actual.Success);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.DisposeAsync"/> removes all active sessions from the internal collection.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DisposeAsync_should_RemoveActiveSessions()
    {
        var session = SessionProvider.Create("Session1");
        _fakeActiveSessions.Add(session.Id, session);
        session = SessionProvider.Create("Session2");
        _fakeActiveSessions.Add(session.Id, session);

        await _sessionService.DisposeAsync();

        Assert.Empty(_fakeActiveSessions);
    }
}
