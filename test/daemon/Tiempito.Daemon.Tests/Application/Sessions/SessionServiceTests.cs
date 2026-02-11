using Microsoft.Extensions.Hosting;

using Moq;

using Tiempito.Daemon.Application.Notifications;
using Tiempito.Daemon.Application.Sessions;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Notifications.Enums;
using Tiempito.Daemon.Domain.Sessions;
using Tiempito.Daemon.Domain.Sessions.Enums;
using Tiempito.Daemon.Domain.Sessions.ValueObjects;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.Daemon.Tests.Sessions.Helpers;
using Tiempito.IPC.Models;

namespace Tiempito.Daemon.Tests.Application.Sessions;

/// <summary>
/// Unit tests for <see cref="SessionService"/> covering session lifecycle operations.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "Session")]
public sealed class SessionServiceTests : IDisposable
{
    private readonly SessionService _sessionService;
    private readonly MockRepository _mockRepository;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IStandardOutQueueWriter> _stdOutQueueWriterMock;
    private readonly Mock<IHostApplicationLifetime> _appLifetimeMock;
    private readonly Mock<ISessionFactory> _sessionFactoryMock;
    private readonly Mock<ISession> _sessionMock;
    private readonly Dictionary<string, ISession> _fakeActiveSessions;
    private readonly SessionState _sessionStateStub;

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
        _sessionMock = _mockRepository.Create<ISession>(MockBehavior.Loose);
        _fakeActiveSessions = new Dictionary<string, ISession>();
        _sessionStateStub = SessionState.CreateInitial(TimeSpan.Zero);

        _sessionService = new SessionService(
            _notificationServiceMock.Object,
            _stdOutQueueWriterMock.Object,
            _appLifetimeMock.Object,
            _sessionFactoryMock.Object,
            _fakeActiveSessions);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Verifies that <see cref="SessionService.StartSessionAsync"/> successfully starts a session
    /// when provided with various combinations of session and configuration IDs.
    /// </summary>
    /// <param name="sessionId">The session ID to use for starting the session.</param>
    /// <param name="configId">The configuration ID to use for the session.</param>
    /// <param name="specifyConfigId">Indicates whether the configuration ID should be explicitly checked for existence.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("sessionId", "configId", true)]
    [InlineData("sessionId", "", false)]
    [InlineData("", "configId", true)]
    [InlineData("", "", false)]
    public async Task StartSessionAsync_should_StartSession(string sessionId, string configId, bool specifyConfigId)
    {
        // Arrange
        _appLifetimeMock.Setup(m => m.ApplicationStopping).Returns(It.IsAny<CancellationToken>());
        _sessionMock.Setup(x => x.Id).Returns(sessionId);
        _sessionMock.Setup(m => m.Start(_appLifetimeMock.Object.ApplicationStopping));
        _sessionFactoryMock.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(_sessionMock.Object);
        _notificationServiceMock.Setup(m => m.NotifyAsync(_sessionMock.Object.State, NotificationType.SessionStarted))
            .Returns(ValueTask.CompletedTask);
        if (specifyConfigId)
        {
            _sessionFactoryMock.Setup(m => m.ExistsConfig(configId)).Returns(true);
        }

        // Act
        OperationResult actual = await _sessionService.StartSessionAsync(sessionId, configId);

        // Assert
        Assert.True(actual.Success);
    }

    /// <summary>
    /// Verifies that <see cref="SessionService.StartSessionAsync"/> triggers a notification
    /// when a session is successfully started.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StartSessionAsync_should_NotifySessionStarted()
    {
        // Arrange
        const string sessionId = "AnotherId";
        _sessionMock.SetupGet(m => m.Id).Returns(sessionId);
        _appLifetimeMock.Setup(m => m.ApplicationStopping).Returns(It.IsAny<CancellationToken>());
        _sessionFactoryMock.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(_sessionMock.Object);
        _notificationServiceMock
            .Setup(m => m.NotifyAsync(_sessionMock.Object.State, NotificationType.SessionStarted))
            .Returns(ValueTask.CompletedTask);

        // Act
        _ = await _sessionService.StartSessionAsync(sessionId);

        // Assert
        _notificationServiceMock.Verify();
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
        const string sessionId = "AnotherId";
        _fakeActiveSessions.Add(sessionId, _sessionMock.Object);

        OperationResult actual = await _sessionService.StartSessionAsync(sessionId);

        Assert.False(actual.Success);
    }

    /// <summary>
    /// Verifies that <see cref="SessionService.StartSessionAsync"/> uses the session configuration ID as a fallback
    /// when the session ID is not specified.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StartSessionAsync_should_UseSessionConfigIdAsFallback_when_SessionIdIsNotSpecified()
    {
        // Arrange
        const string sessionConfigId = "Test ID";
        _sessionMock.Setup(x => x.Id).Returns(sessionConfigId);
        _sessionFactoryMock.Setup(m => m.ExistsConfig(sessionConfigId)).Returns(true);
        _appLifetimeMock.Setup(m => m.ApplicationStopping).Returns(It.IsAny<CancellationToken>());
        _sessionFactoryMock.Setup(m => m.Create(sessionConfigId, sessionConfigId)).Returns(_sessionMock.Object);
        _notificationServiceMock
            .Setup(m => m.NotifyAsync(_sessionMock.Object.State, NotificationType.SessionStarted))
            .Returns(ValueTask.CompletedTask);

        // Act
        _ = await _sessionService.StartSessionAsync(sessionConfigId: sessionConfigId);

        // Assert
        _sessionFactoryMock.Verify(m => m.Create(sessionConfigId, sessionConfigId));
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
        const string idStub = "AnotherId";
        var state = _sessionStateStub.WithStatus(SessionStatus.Executing);
        _sessionMock.Setup(m => m.State).Returns(state);
        _sessionMock.Setup(m => m.Pause());
        _fakeActiveSessions.Add(idStub, _sessionMock.Object);
        string sessionId = specifySessionId ? idStub : string.Empty;

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
        const string idStub = "AnotherId";
        var state = _sessionStateStub.WithStatus(SessionStatus.Paused);
        _sessionMock.Setup(m => m.State).Returns(state);
        _sessionMock.Setup(m => m.Resume());
        _fakeActiveSessions.Add(idStub, _sessionMock.Object);
        string sessionId = specifySessionId ? idStub : string.Empty;

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
    /// Tests that <see cref="SessionService.CancelSession"/> successfully cancels a running session
    /// when the session ID is specified or not.
    /// </summary>
    /// <param name="specifySessionId">Indicates whether the session ID is explicitly provided.</param>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CancelSession_should_CancelActiveSession(bool specifySessionId)
    {
        const string idStub = "AnotherId";
        _sessionMock.Setup(m => m.Id).Returns(idStub).Verifiable(Times.AtMostOnce);
        _sessionMock.Setup(m => m.Cancel());
        _fakeActiveSessions.Add(idStub, _sessionMock.Object);
        string sessionId = specifySessionId ? idStub : string.Empty;

        OperationResult actual = _sessionService.CancelSession(sessionId);

        Assert.True(actual.Success, actual.Message);
        Assert.DoesNotContain(_fakeActiveSessions, s => s.Key == idStub);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.CancelSession"/> returns an error when there are no sessions to cancel.
    /// </summary>
    [Fact]
    public void CancelSession_return_Error_when_NoSessionsToCancel()
    {
        OperationResult operationResult = _sessionService.CancelSession();

        Assert.False(operationResult.Success);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.CancelSession"/> returns a failed operation
    /// when the specified session ID is not found.
    /// </summary>
    [Fact]
    public void CancelSession_should_ReturnFailedOperation_when_IdNotFound()
    {
        const string stubId = "AnotherId";

        OperationResult actual = _sessionService.CancelSession(stubId);

        Assert.False(actual.Success);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.Dispose"/> removes all active sessions from the internal collection.
    /// </summary>
    [Fact]
    public void Dispose_should_DisposeAndRemoveActiveSessions()
    {
        // Arrange
        var sessionMock2 = _mockRepository.Create<ISession>(MockBehavior.Loose);
        _fakeActiveSessions.Add("1", _sessionMock.Object);
        _fakeActiveSessions.Add("2", sessionMock2.Object);

        // Act
        _sessionService.Dispose();

        // Assert
        Assert.Empty(_fakeActiveSessions);
        _sessionMock.Verify(m => m.Dispose(), Times.Once);
        sessionMock2.Verify(m => m.Dispose(), Times.Once);
        Assert.Null(_sessionMock.Object.SecondElapsedAsync);
        Assert.Null(_sessionMock.Object.IntervalCompletedAsync);
        Assert.Null(_sessionMock.Object.CompletedAsync);
        Assert.Null(sessionMock2.Object.SecondElapsedAsync);
        Assert.Null(sessionMock2.Object.IntervalCompletedAsync);
        Assert.Null(sessionMock2.Object.CompletedAsync);
    }

    /// <summary>
    /// Verifies that <see cref="SessionService"/> sends a session progress message
    /// when a second elapses during a session.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SessionService_should_SendSessionProgress_when_SecondElapsed()
    {
        // Arrange
        const string idStub = "AnotherId";
        Func<ISession, ValueTask>? eventHandler = null;
        _sessionMock.Setup(m => m.Id).Returns(idStub);
        _sessionMock.Setup(m => m.State).Returns(SessionState.CreateInitial(TimeSpan.FromMinutes(1)));
        _sessionMock.SetupSet(m => m.SecondElapsedAsync = It.IsAny<Func<ISession, ValueTask>>())
            .Callback<Func<ISession, ValueTask>>(handler => eventHandler = handler);
        _sessionFactoryMock.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(_sessionMock.Object);
        _appLifetimeMock.Setup(m => m.ApplicationStopping).Returns(It.IsAny<CancellationToken>());
        _notificationServiceMock
            .Setup(m => m.NotifyAsync(_sessionMock.Object.State, NotificationType.SessionStarted))
            .Returns(ValueTask.CompletedTask);
        _stdOutQueueWriterMock.Setup(m => m.WriteAsync(It.IsAny<SessionProgressMessage>()))
            .Returns(ValueTask.CompletedTask);
        await _sessionService.StartSessionAsync(idStub);

        // Act
        if (eventHandler is not null)
        {
            await eventHandler(_sessionMock.Object);
        }

        // Assert
        _stdOutQueueWriterMock.Verify(m => m.WriteAsync(It.IsAny<SessionProgressMessage>()), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="SessionService"/> notifies when a session interval completes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SessionService_should_NotifyIntervalCompleted_when_IntervalCompletes()
    {
        const string idStub = "AnotherId";
        Func<ISession, ValueTask>? eventHandler = null;
        _sessionMock.Setup(m => m.Id).Returns(idStub);
        _sessionMock.Setup(m => m.State).Returns(_sessionStateStub);
        _sessionMock.SetupSet(m => m.IntervalCompletedAsync = It.IsAny<Func<ISession, ValueTask>>())
            .Callback<Func<ISession, ValueTask>>(handler => eventHandler = handler);
        _sessionFactoryMock.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(_sessionMock.Object);
        _appLifetimeMock.Setup(m => m.ApplicationStopping).Returns(It.IsAny<CancellationToken>());
        _notificationServiceMock
            .Setup(m => m.NotifyAsync(_sessionMock.Object.State, NotificationType.SessionStarted))
            .Returns(ValueTask.CompletedTask);
        _notificationServiceMock
            .Setup(m => m.NotifyAsync(_sessionMock.Object.State, NotificationType.SessionIntervalCompleted))
            .Returns(ValueTask.CompletedTask);
        _stdOutQueueWriterMock.Setup(m => m.WriteAsync(It.IsAny<SessionProgressMessage>()))
            .Returns(ValueTask.CompletedTask);
        await _sessionService.StartSessionAsync(idStub);

        if (eventHandler is not null)
        {
            await eventHandler(_sessionMock.Object);
        }

        _notificationServiceMock.Verify(
            m => m.NotifyAsync(_sessionMock.Object.State, NotificationType.SessionIntervalCompleted),
            Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="SessionService"/> notifies when a session is completed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SessionService_should_NotifySessionCompleted_when_SessionCompletes()
    {
        const string idStub = "AnotherId";
        Func<ISession, ValueTask>? eventHandler = null;
        _sessionMock.Setup(m => m.Id).Returns(idStub);
        _sessionMock.Setup(m => m.State).Returns(_sessionStateStub);
        _sessionMock.SetupSet(m => m.CompletedAsync = It.IsAny<Func<ISession, ValueTask>>())
            .Callback<Func<ISession, ValueTask>>(handler => eventHandler = handler);
        _sessionFactoryMock.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(_sessionMock.Object);
        _appLifetimeMock.Setup(m => m.ApplicationStopping).Returns(It.IsAny<CancellationToken>());
        _notificationServiceMock
            .Setup(m => m.NotifyAsync(_sessionMock.Object.State, NotificationType.SessionStarted))
            .Returns(ValueTask.CompletedTask);
        _notificationServiceMock
            .Setup(m => m.NotifyAsync(_sessionMock.Object.State, NotificationType.SessionCompleted))
            .Returns(ValueTask.CompletedTask);
        _stdOutQueueWriterMock.Setup(m => m.WriteAsync(It.IsAny<SessionProgressMessage>()))
            .Returns(ValueTask.CompletedTask);
        await _sessionService.StartSessionAsync(idStub);

        if (eventHandler is not null)
        {
            await eventHandler(_sessionMock.Object);
        }

        _notificationServiceMock.Verify(
            m => m.NotifyAsync(_sessionMock.Object.State, NotificationType.SessionCompleted),
            Times.Once);
    }
}
