using Moq;

using Tiempito.CLI.Services.Abstractions;
using Tiempito.CLI.Services.Implementations;
using Tiempito.IPC.Models;
using Tiempito.IPC.Models.Enums;

namespace Tiempito.CLI.UnitTests.Services.Implementations;

/// <summary>
/// Unit tests for the <see cref="SessionFollower"/> class, verifying its behavior in various scenarios.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature ", "CLI")]
public sealed class SessionFollowerTests : IDisposable
{
    private static readonly CancellationToken CancellationToken = CancellationToken.None;
    private static readonly SessionProgressMessage SessionProgressMsg = SessionProgressMessage.CreateNew(
        sessionId: "1",
        intervalDuration: TimeSpan.FromSeconds(5),
        intervalType: SessionIntervalType.Focus,
        cycle: 1,
        elapsedTime: TimeSpan.FromSeconds(2),
        intervalCompleted: false,
        sessionCompleted: false);

    private readonly MockRepository _mockRepository;
    private readonly Mock<IClient> _clientMock;
    private readonly Mock<IMessageWriter> _messageWriterMock;
    private readonly SessionFollower _follower;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionFollowerTests"/> class.
    /// </summary>
    public SessionFollowerTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Loose);
        _clientMock = _mockRepository.Create<IClient>();
        _messageWriterMock = _mockRepository.Create<IMessageWriter>();
        _follower = new SessionFollower(_clientMock.Object, _messageWriterMock.Object);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Tests that <see cref="SessionFollower.FollowAsync"/> reads incoming messages
    /// when <c>MustFollow</c> is <c>true</c> and cancellation is not requested.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task FollowAsync_should_ReadIncomingMessages_when_FollowIsTrueAndCancellationIsNotRequested()
    {
        // Arrange
        _follower.MustFollow = true;
        _clientMock.SetupSequence(m => m.ReceiveMessageAsync<Message>(false, CancellationToken))
            .ReturnsAsync(SessionProgressMsg)
            .ReturnsAsync(ConnectionTerminationMessage.CreateNew());

        // Act
        await _follower.FollowAsync(CancellationToken);

        // Assert
        _messageWriterMock.Verify(m => m.ClearLineAsync(CancellationToken), Times.Once);
        _messageWriterMock.Verify(
            m => m.WriteAsync(error: false, It.IsAny<string>(), CancellationToken), Times.Once);
        _messageWriterMock.Verify(
            m => m.WriteLineAsync(error: false, It.IsAny<string>(), CancellationToken), Times.AtLeastOnce);
    }

    /// <summary>
    /// Tests that <see cref="SessionFollower.FollowAsync"/> does not read incoming messages
    /// when <c>MustFollow</c> is <c>false</c>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task FollowAsync_should_NotReadIncomingMessages_when_FollowIsFalse()
    {
        // Arrange
        _follower.MustFollow = false;

        // Act
        await _follower.FollowAsync(CancellationToken);

        // Assert
        _clientMock.Verify(m => m.ReceiveMessageAsync<Message>(It.IsAny<bool>(), CancellationToken), Times.Never);
        _messageWriterMock.Verify(m => m.ClearLineAsync(CancellationToken), Times.Never);
        _messageWriterMock.Verify(
            m => m.WriteAsync(It.IsAny<bool>(), It.IsAny<string>(), CancellationToken), Times.Never);
        _messageWriterMock.Verify(
            m => m.WriteLineAsync(It.IsAny<bool>(), It.IsAny<string>(), CancellationToken), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="SessionFollower.FollowAsync"/> stops reading incoming messages when cancellation is requested.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task FollowAsync_should_StopReadingIncomingMessages_when_CancellationIsRequested()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        _follower.MustFollow = true;
        _clientMock.SetupSequence(m => m.ReceiveMessageAsync<Message>(false, cts.Token))
            .ReturnsAsync(SessionProgressMsg)
            .ReturnsAsync(() =>
            {
                cts.Cancel();
                return SessionProgressMsg;
            });

        // Act
        await _follower.FollowAsync(cts.Token);

        // Assert
        _messageWriterMock.Verify(m => m.ClearLineAsync(cts.Token), Times.Exactly(2));
        _messageWriterMock.Verify(m => m.WriteAsync(It.IsAny<bool>(), It.IsAny<string>(), cts.Token), Times.Exactly(2));
        _messageWriterMock.Verify(m => m.WriteLineAsync(It.IsAny<bool>(), It.IsAny<string>(), cts.Token), Times.Once);
    }
}