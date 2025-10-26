using Tiempito.CLI.Services.Abstractions;

namespace Tiempito.CLI.Services.Implementations;

/// <summary>
/// Implements <c>ISessionFollower</c> to follow session messages from a client and write them using a message writer.
/// </summary>
internal sealed class SessionFollower : ISessionFollower
{
    /// <inheritdoc/>
    public bool MustFollow { private get; set; }

    private readonly IClient _client;
    private readonly IMessageWriter _messageWriter;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionFollower"/> class.
    /// </summary>
    /// <param name="client">The client to read session messages from.</param>
    /// <param name="messageWriter">The message writer to output messages.</param>
    public SessionFollower(IClient client, IMessageWriter messageWriter)
    {
        _client = client;
        _messageWriter = messageWriter;
    }

    /// <inheritdoc/>
    public async ValueTask FollowAsync(CancellationToken cancellationToken = default)
    {
        if (!MustFollow)
        {
            return;
        }

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                string message = await _client.ReadPipeStdInAsync(cancellationToken);
                await _messageWriter.WriteAsync(error: false, message, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }
}