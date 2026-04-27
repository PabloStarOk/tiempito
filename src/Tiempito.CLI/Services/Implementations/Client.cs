using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;

using Tiempito.CLI.Exceptions;
using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Abstractions;
using Tiempito.IPC.Models;

using IMessageWriter = Tiempito.IPC.Abstractions.IMessageWriter;

namespace Tiempito.CLI.Services.Implementations;

/// <summary>
/// Client that sends requests to the daemon and receive responses from the daemon.
/// </summary>
public sealed class Client : IClient, IAsyncDisposable
{
    private const int ConnectionTimeout = 3000;
    private readonly NamedPipeClientStream _pipeClient;
    private readonly IMessageWriter _messageWriter;
    private readonly IMessageReader _messageReader;
    private readonly List<Message> _messagesBuffer = [];
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="pipeClient">The named pipe client stream used to connect to the daemon.</param>
    /// <param name="messageWriter">The message writer for sending requests.</param>
    /// <param name="messageReader">The message reader for receiving responses.</param>
    public Client(
        NamedPipeClientStream pipeClient,
        IMessageWriter messageWriter,
        IMessageReader messageReader)
    {
        _pipeClient = pipeClient;
        _messageWriter = messageWriter;
        _messageReader = messageReader;
    }

    /// <inheritdoc/>
    public async Task SendMessageAsync(Message command, CancellationToken cancellationToken = default)
    {
        if (!_pipeClient.IsConnected)
        {
            await _pipeClient.ConnectAsync(ConnectionTimeout, cancellationToken);
        }

        await _messageWriter.WriteAsync(_pipeClient, command, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TMessage> ReceiveMessageAsync<TMessage>(
        bool useTimeout,
        CancellationToken cancellationToken = default)
        where TMessage : Message
    {
        if (!_pipeClient.IsConnected)
        {
            throw new InvalidOperationException("Named pipe is not connected.");
        }

        if (TryGetBufferedMessage(out TMessage? bufferedMessage))
        {
            return bufferedMessage;
        }

        using var timeoutCts = new CancellationTokenSource(useTimeout ? ConnectionTimeout : Timeout.Infinite);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        Message? message = null;
        while (message is not TMessage)
        {
            if (message is not null)
            {
                _messagesBuffer.Add(message);
            }

            try
            {
                message = await _messageReader.ReadAsync<Message>(_pipeClient, linkedCts.Token);
            }
            catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
            {
                throw new ResponseTimeoutException();
            }
        }

        return (TMessage)message;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (!_pipeClient.IsConnected)
        {
            return;
        }

        try
        {
            await SendMessageAsync(ConnectionTerminationMessage.CreateNew(), CancellationToken.None);
        }
        catch (IOException)
        {
            // Ignore, termination message already sent by the daemon.
        }
    }

    private bool TryGetBufferedMessage<TMessage>([NotNullWhen(true)] out TMessage? message)
    {
        message = default;
        if (_messagesBuffer.Count is 0)
        {
            return false;
        }

        for (int i = 0; i < _messagesBuffer.Count; i++)
        {
            if (_messagesBuffer[i] is not TMessage foundInQueue)
            {
                continue;
            }

            _messagesBuffer.RemoveAt(i);
            message = foundInQueue;
            return true;
        }

        return false;
    }
}
