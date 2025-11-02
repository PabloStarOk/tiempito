using System.IO.Pipes;

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
    public async Task<TMessage> ReceiveMessageAsync<TMessage>(CancellationToken cancellationToken = default)
        where TMessage : Message
    {
        if (!_pipeClient.IsConnected)
        {
            throw new InvalidOperationException("Named pipe is not connected.");
        }

        var message = await _messageReader.ReadAsync<Message>(_pipeClient, cancellationToken);
        if (message is not TMessage typedMessage)
        {
            throw new InvalidOperationException($"Received message is not of expected type {typeof(TMessage).FullName}.");
        }

        return typedMessage;
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
}
