using System.Threading.Channels;

using Tiempito.Daemon.Application.Notifications;
using Tiempito.IPC.Models;

namespace Tiempito.Daemon.Infrastructure.Notifications;

/// <summary>
/// Provides a thread-safe queue for standard output messages using <see cref="Channel{T}"/>.
/// </summary>
internal sealed class StandardOutQueue : IStandardOutQueueWriter, IStandardOutQueueReader
{
    /// <inheritdoc/>
    public ChannelReader<Message> Reader => _channel.Reader;

    private readonly Channel<Message> _channel;

    /// <summary>
    /// Initializes a new instance of the <see cref="StandardOutQueue"/> class with the specified channel.
    /// </summary>
    /// <param name="channel">The channel used for thread-safe message queuing.</param>
    public StandardOutQueue(Channel<Message> channel)
    {
        _channel = channel;
    }

    /// <inheritdoc/>
    public async ValueTask WriteAsync(Message message)
    {
        await _channel.Writer.WriteAsync(message);
    }
}