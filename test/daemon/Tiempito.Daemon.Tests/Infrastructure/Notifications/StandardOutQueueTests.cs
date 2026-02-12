using System.Threading.Channels;

using Tiempito.Daemon.Infrastructure.Notifications;
using Tiempito.IPC.Models;

namespace Tiempito.Daemon.Tests.Infrastructure.Notifications;

/// <summary>
/// Unit tests for the <see cref="StandardOutQueue"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "Notifications")]
public sealed class StandardOutQueueTests
{
    private readonly Channel<Message> _channel;
    private readonly StandardOutQueue _queue;

    /// <summary>
    /// Initializes a new instance of the <see cref="StandardOutQueueTests"/> class.
    /// </summary>
    public StandardOutQueueTests()
    {
        _channel = Channel.CreateUnbounded<Message>();
        _queue = new StandardOutQueue(_channel);
    }

    /// <summary>
    /// Tests that the Reader property returns the channel's reader.
    /// </summary>
    [Fact]
    public void Reader_should_ReturnChannelReader()
    {
        // Act
        ChannelReader<Message> actual = _queue.Reader;

        // Assert
        Assert.Equal(_channel.Reader, actual);
    }

    /// <summary>
    /// Tests that WriteAsync writes a message to the channel.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task WriteAsync_should_WriteMessageToChannel()
    {
        // Arrange
        var msg = ConnectionTerminationMessage.CreateNew();

        // Act
        await _queue.WriteAsync(msg);

        // Assert
        Assert.True(_channel.Reader.TryRead(out Message? actualMessage));
        Assert.Equal(msg, actualMessage);
    }
}