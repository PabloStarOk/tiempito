using System.IO.Pipes;

using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Abstractions;
using Tiempito.IPC.Models;

using IMessageWriter = Tiempito.IPC.Abstractions.IMessageWriter;

namespace Tiempito.CLI.Services.Implementations;

/// <summary>
/// Client that sends requests to the daemon and receive responses from the daemon.
/// </summary>
public class Client : IClient
{
    private const int ConnectionTimeout = 3000;
    private readonly NamedPipeClientStream _pipeClient;
    private readonly IMessageWriter _messageWriter;
    private readonly IMessageReader _messageReader;

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
    public async Task SendCommandAsync(Command command, CancellationToken cancellationToken = default)
    {
        if (!_pipeClient.IsConnected)
        {
            await _pipeClient.ConnectAsync(ConnectionTimeout, cancellationToken);
        }

        await _messageWriter.WriteAsync(_pipeClient, command, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Response?> ReceiveResponseAsync(CancellationToken cancellationToken = default)
    {
        if (!_pipeClient.IsConnected)
        {
            throw new InvalidOperationException("Named pipe is not connected.");
        }

        var response = await _messageReader.ReadAsync<Response>(_pipeClient, cancellationToken);
        return response ?? throw new InvalidOperationException("Response not recognized.");
    }

    /// <inheritdoc/>
    public async Task<string> ReadPipeStdInAsync(CancellationToken cancellationToken = default)
    {
        string? message = await _messageReader.ReadAsync<string>(_pipeClient, cancellationToken);
        return message ?? throw new InvalidOperationException("Message not recognized.");
    }
}
