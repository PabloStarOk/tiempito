using System.Threading.Channels;

namespace Tiempito.Daemon.Application.Notifications;

/// <summary>
/// Represents a reader for the standard output queue.
/// </summary>
public interface IStandardOutQueueReader
{
    /// <summary>
    /// Gets the channel reader for reading standard output messages.
    /// </summary>
    public ChannelReader<string> Reader { get; }
}