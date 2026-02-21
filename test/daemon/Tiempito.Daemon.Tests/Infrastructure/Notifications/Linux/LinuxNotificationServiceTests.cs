using System.IO.Abstractions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Tiempito.Daemon.Application.Notifications;
using Tiempito.Daemon.Application.Shared;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Notifications.Enums;
using Tiempito.Daemon.Domain.Sessions.ValueObjects;
using Tiempito.Daemon.Infrastructure.Notifications.Linux;
using Tiempito.IPC.Models.Enums;

namespace Tiempito.Daemon.Tests.Infrastructure.Notifications.Linux;

/// <summary>
/// Unit tests for the <see cref="LinuxNotificationService"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "Notifications")]
public sealed class LinuxNotificationServiceTests : IDisposable
{
    private static readonly NotificationConfig NotificationConfig = new ()
    {
        AppName = "TestApp",
        IconPath = Paths.ApplicationIconPath,
        ExpirationTimeoutMs = 2000,
    };

    private readonly MockRepository _mockRepository;
    private readonly Mock<IOptionsMonitor<UserConfig>> _userConfigMonitorMock;
    private readonly Mock<ILinuxNotifier> _notifierMock;
    private readonly Mock<ILinuxSoundPlayer> _soundPlayerMock;
    private readonly Mock<ILinuxNotificationIconLoader> _iconLoaderMock;
    private readonly Mock<IFileSystem> _fileSystemMock;
    private readonly LinuxNotificationService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="LinuxNotificationServiceTests"/> class.
    /// </summary>
    public LinuxNotificationServiceTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Loose);
        var loggerMock = _mockRepository.Create<ILogger<LinuxNotificationService>>();
        var notificationOptionsMock = _mockRepository.Create<IOptionsMonitor<NotificationConfig>>();
        _userConfigMonitorMock = _mockRepository.Create<IOptionsMonitor<UserConfig>>();
        _notifierMock = _mockRepository.Create<ILinuxNotifier>();
        _soundPlayerMock = _mockRepository.Create<ILinuxSoundPlayer>();
        _iconLoaderMock = _mockRepository.Create<ILinuxNotificationIconLoader>();
        _fileSystemMock = _mockRepository.Create<IFileSystem>();
        notificationOptionsMock.Setup(m => m.CurrentValue).Returns(NotificationConfig);
        _service = new LinuxNotificationService(
            loggerMock.Object,
            notificationOptionsMock.Object,
            _userConfigMonitorMock.Object,
            _notifierMock.Object,
            _soundPlayerMock.Object,
            _iconLoaderMock.Object,
            _fileSystemMock.Object);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Tests that <see cref="LinuxNotificationService.StartAsync"/> loads the application icon when it exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StartAsync_should_LoadIcon_when_IconExists()
    {
        // Arrange
        var imgData = new LinuxNotificationImageData
        {
            Width = 300,
            Height = 300,
            Channels = 4,
            BitsPerSample = 8,
            Data = "Some fake RGB data"u8.ToArray(),
            HasAlpha = true,
            RowStride = 8,
        };
        _fileSystemMock.Setup(m => m.Path.Exists(Paths.ApplicationIconPath)).Returns(true);
        _iconLoaderMock.Setup(m => m.LoadAsync(Paths.ApplicationIconPath)).ReturnsAsync(imgData);

        // Act
        await _service.StartAsync(CancellationToken.None);

        // Assert
        _iconLoaderMock.Verify(m => m.LoadAsync(Paths.ApplicationIconPath), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="LinuxNotificationService.StartAsync"/> does not load the application icon when it does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task StartAsync_should_NotLoadIcon_when_IconDoesNotExist()
    {
        // Arrange
        _fileSystemMock.Setup(m => m.Path.Exists(Paths.ApplicationIconPath)).Returns(false);

        // Act
        await _service.StartAsync(CancellationToken.None);

        // Assert
        _iconLoaderMock.Verify(m => m.LoadAsync(It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="LinuxNotificationService.NotifyAsync"/> notifies correctly when notifications are enabled.
    /// </summary>
    /// <param name="type">The type of notification.</param>
    /// <param name="expectedSummary">The expected summary of the notification.</param>
    /// <param name="expectedBody">The expected body of the notification.</param>
    /// <param name="expectedSoundFile">The expected sound file of the notification.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData(NotificationType.SessionStarted, "Session started", "A new session was started.", "session-alarm.wav")]
    [InlineData(NotificationType.SessionCompleted, "Session finished", "Session finished.", "session-alarm.wav")]
    [InlineData(NotificationType.SessionIntervalCompleted, "Focus completed", "A focus time was completed.", "time-completed-alarm.wav")]
    public async Task NotifyAsync_should_NotifyCorrectly_when_NotificationsEnabled(
        NotificationType type,
        string expectedSummary,
        string expectedBody,
        string expectedSoundFile)
    {
        // Arrange
        var sessionState = SessionState.CreateInitial(TimeSpan.FromMinutes(25));
        var userConfig = new UserConfig(null, [UserFeature.Notification]);
        _fileSystemMock.Setup(m => m.Path.Combine(Paths.DaemonConfigDirectoryPath, expectedSoundFile))
            .Returns(expectedSoundFile);
        _userConfigMonitorMock.Setup(m => m.CurrentValue).Returns(userConfig);

        // Act
        await _service.NotifyAsync(sessionState, type);

        // Assert
        _notifierMock.Verify(n => n.CloseLastAsync(), Times.Once);
        _soundPlayerMock.Verify(m => m.PlayAsync(It.Is<string>(path => path.EndsWith(expectedSoundFile))), Times.Once);
        _notifierMock.Verify(
            m => m.NotifyAsync(It.Is<LinuxNotification>(n =>
                n.ApplicationName == NotificationConfig.AppName
                && n.Icon == NotificationConfig.IconPath
                && n.ExpirationTimeout == NotificationConfig.ExpirationTimeoutMs
                && n.Summary == expectedSummary
                && n.Body == expectedBody
                && n.AudioFilePath == expectedSoundFile)),
            Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="LinuxNotificationService.NotifyAsync"/> does not notify when notifications are disabled.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task NotifyAsync_should_NotNotify_when_NotificationsAreDisabled()
    {
        // Arrange
        var sessionState = SessionState.CreateInitial(TimeSpan.FromMinutes(25));
        var userConfig = new UserConfig("Test", []);
        _userConfigMonitorMock.Setup(x => x.CurrentValue).Returns(userConfig);

        // Act
        await _service.NotifyAsync(sessionState, NotificationType.SessionStarted);

        // Assert
        _notifierMock.Verify(n => n.CloseLastAsync(), Times.Never);
        _notifierMock.Verify(n => n.NotifyAsync(It.IsAny<LinuxNotification>()), Times.Never);
        _soundPlayerMock.Verify(s => s.PlayAsync(It.IsAny<string>()), Times.Never);
    }
}
