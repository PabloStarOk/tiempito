using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

using Moq;

using Tiempito.Daemon.Application.Config.Sessions;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Sessions;
using Tiempito.Daemon.Domain.Sessions.Enums;
using Tiempito.Daemon.Infrastructure.Sessions;
using Tiempito.Daemon.Tests.Sessions.Helpers;

namespace Tiempito.Daemon.Tests.Infrastructure.Sessions;

/// <summary>
/// Unit tests for <see cref="SessionFactory"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "Session")]
public sealed class SessionFactoryTests : IDisposable
{
    private readonly MockRepository _mockRepository;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<ISessionConfigService> _sessionConfigServiceMock;
    private readonly SessionFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionFactoryTests"/> class.
    /// </summary>
    public SessionFactoryTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Loose);
        Mock<ILogger<SessionFactory>> loggerMock = _mockRepository.Create<ILogger<SessionFactory>>();
        _loggerFactoryMock = _mockRepository.Create<ILoggerFactory>();
        var fakeTimeProvider = new FakeTimeProvider();
        _sessionConfigServiceMock = _mockRepository.Create<ISessionConfigService>();
        _factory = new SessionFactory(
            loggerMock.Object, _loggerFactoryMock.Object, fakeTimeProvider, _sessionConfigServiceMock.Object);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Tests that <see cref="SessionFactory.Create"/> returns a session with expected initial values.
    /// </summary>
    [Fact]
    public void Create_should_ReturnExpectedSessionWithInitialValues()
    {
        // Arrange
        const string id = "session1";
        const string configId = "config1";
        SessionConfig? sessionConfig = SessionProvider.CreateConfig(configId);
        _loggerFactoryMock.Setup(m => m.CreateLogger(typeof(Session).FullName!))
            .Returns(new Mock<ILogger<Session>>().Object);
        _sessionConfigServiceMock.Setup(m => m.TryGetConfigById(configId, out sessionConfig)).Returns(true);

        // Act
        ISession session = _factory.Create(id, configId);

        // Assert
        Assert.Equal(id, session.Id);
        Assert.Equal(sessionConfig, session.Configuration);
        Assert.Equal(SessionStatus.None, session.State.Status);
        Assert.Equal(0, session.State.Cycle);
        Assert.Equal(SessionIntervalType.Focus, session.State.IntervalType);
        Assert.Equal(TimeSpan.Zero, session.State.ElapsedTime);
    }

    /// <summary>
    /// Tests that <see cref="SessionFactory.Create"/> uses the configId as the sessionId
    /// when the given sessionId is null, empty, or whitespace.
    /// </summary>
    /// <param name="sessionId">The session ID to use, which may be null, empty, or whitespace.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_should_UseConfigIdAsSessionId_when_GivenIdIsNullEmpty(string? sessionId)
    {
        // Arrange
        const string configId = "config1";
        SessionConfig? sessionConfig = SessionProvider.CreateConfig(configId);
        _loggerFactoryMock.Setup(m => m.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger<Session>>().Object);
        _sessionConfigServiceMock.Setup(m => m.TryGetConfigById(configId, out sessionConfig)).Returns(true);

        // Act
        ISession session = _factory.Create(sessionId, configId);

        // Assert
        Assert.Equal(configId, session.Id);
    }

    /// <summary>
    /// Tests that <see cref="SessionFactory.Create"/> uses the default configuration
    /// when the given <paramref name="configId"/> is null, empty, or whitespace.
    /// </summary>
    /// <param name="configId">The configuration ID to use for session creation, which may be null, empty, or whitespace.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_should_UseDefaultConfig_when_GivenConfigIdIsEmpty(string? configId)
    {
        // Arrange
        const string id = "session1";
        SessionConfig sessionConfig = SessionProvider.CreateConfig();
        _loggerFactoryMock.Setup(m => m.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger<Session>>().Object);
        _sessionConfigServiceMock.Setup(m => m.DefaultConfig).Returns(sessionConfig);

        // Act
        ISession session = _factory.Create(id, configId);

        // Assert
        Assert.Equal(sessionConfig, session.Configuration);
    }

    /// <summary>
    /// Tests that <see cref="SessionFactory.Create"/> throws an <see cref="ArgumentException"/>
    /// when a configId is provided but does not exist.
    /// </summary>
    [Fact]
    public void Create_should_ThrowArgumentException_when_ConfigIdIsProvidedButDoesNotExist()
    {
        // Arrange
        const string id = "session1";
        const string configId = "config1";
        SessionConfig? sessionConfig = null;
        _sessionConfigServiceMock.Setup(m => m.TryGetConfigById(configId, out sessionConfig)).Returns(false);

        // Assert
        Assert.Throws<ArgumentException>(() => _factory.Create(id, configId));
    }

    /// <summary>
    /// Tests that <see cref="SessionFactory.ExistsConfig"/> returns the expected boolean value
    /// depending on whether the configuration exists.
    /// </summary>
    /// <param name="exists">Whether the configuration is expected to exist.</param>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ExistsConfig_should_ReturnExpectedBoolean(bool exists)
    {
        // Arrange
        const string configId = "config1";
        SessionConfig? sessionConfig = exists ? SessionProvider.CreateConfig() : null;
        _sessionConfigServiceMock.Setup(m => m.TryGetConfigById(configId, out sessionConfig)).Returns(exists);

        // Act
        bool actual = _factory.ExistsConfig(configId);

        // Assert
        Assert.Equal(exists, actual);
    }

    /// <summary>
    /// Tests that <see cref="SessionFactory.ExistsConfig"/> throws an <see cref="ArgumentException"/>
    /// when the provided <paramref name="configId"/> is null, empty, or whitespace.
    /// </summary>
    /// <param name="configId">The configuration ID to check for existence.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ExistsConfig_should_ReturnArgumentException_when_IdIsEmpty(string? configId)
    {
        // Act
        Assert.ThrowsAny<ArgumentException>(() => _factory.ExistsConfig(configId!));
    }
}