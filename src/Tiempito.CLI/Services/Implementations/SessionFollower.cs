using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Models;

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
                var message = await _client.ReceiveMessageAsync<Message>(cancellationToken);
                await HandleMessageAsync(message, cancellationToken);
                if (message is ConnectionTerminationMessage)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }

    private static string GetSessionProgressString(SessionProgressMessage message)
    {
        if (message.IntervalCompleted)
        {
            return $"{message.IntervalType.ToString()} interval completed";
        }

        return message.SessionCompleted
            ? "Session completed"
            : $"{message.IntervalType.ToString()} interval: {message.ElapsedTime}/{message.IntervalDuration}";
    }

    private async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
        bool error = false;
        string outputMsg;
        switch (message)
        {
            case SessionProgressMessage progressMsg:
                outputMsg = GetSessionProgressString(progressMsg);
                break;

            case ConnectionTerminationMessage:
                outputMsg = "Daemon has terminated the connection.";
                break;

            default:
                error = true;
                outputMsg = "Received an unknown message from daemon.";
                break;
        }

        await _messageWriter.WriteAsync(error, outputMsg, cancellationToken);
    }
}