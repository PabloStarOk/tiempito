using System.IO.Pipes;
using Microsoft.Extensions.Options;

using Tiempito.Daemon.Application.Commands;
using Tiempito.Daemon.Application.Notifications;
using Tiempito.Daemon.Server.Configuration;
using Tiempito.IPC.Abstractions;
using Tiempito.IPC.Models;

namespace Tiempito.Daemon.Server;

/// <summary>
/// Represents the server to receive requests and send responses to the client.
/// </summary>
public sealed class Server : BackgroundService, IAsyncDisposable
{
    private readonly ILogger<Server> _logger;
    private readonly PipeConfig _pipeConfig;
    private readonly NamedPipeServerStream _pipeServer;
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IStandardOutQueueReader _stdOutQueueReader;
    private readonly IMessageWriter _messageWriter;
    private readonly IMessageReader _messageReader;
    private string _currentConnectedUser = string.Empty;
    private Task? _stdOutMessagesSendTask;

    public Server(
        ILogger<Server> logger,
        IOptions<PipeConfig> daemonConfigOptions,
        NamedPipeServerStream pipeServer,
        IStandardOutQueueReader stdOutQueueReader,
        ICommandDispatcher commandDispatcher,
        IMessageWriter messageWriter,
        IMessageReader messageReader)
    {
        _logger = logger;
        _pipeConfig = daemonConfigOptions.Value;
        _pipeServer = pipeServer;
        _stdOutQueueReader = stdOutQueueReader;
        _commandDispatcher = commandDispatcher;
        _messageWriter = messageWriter;
        _messageReader = messageReader;
    }

    /// <inheritdoc/>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Disconnect();
        await base.StopAsync(cancellationToken);
        _logger.LogInformation("Server stopped at {Time}", DateTimeOffset.UtcNow);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        Disconnect();

        if (_stdOutMessagesSendTask is not null && !_stdOutMessagesSendTask.IsCompleted)
        {
            try
            {
                await _stdOutMessagesSendTask;
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while disposing std-out-messages send task");
            }
            finally
            {
                _stdOutMessagesSendTask.Dispose();
            }
        }

        await _pipeServer.DisposeAsync();
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Server started at {Time}", DateTimeOffset.UtcNow);
        _stdOutMessagesSendTask = SendStandardOutMessagesAsync(stoppingToken);
        await RunAsync(stoppingToken);
    }

    /// <summary>
    /// Runs the server to connect and disconnect from the client and handle requests.
    /// </summary>
    private async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!_pipeServer.IsConnected)
                await ConnectAsync(cancellationToken);

            Command? command = await _messageReader.ReadAsync<Command>(_pipeServer, cancellationToken);

            if (command is null) // TODO: Replace with a termination request.
            {
                Disconnect();
                continue;
            }

            Response response = await _commandDispatcher.DispatchAsync(command, cancellationToken);
            await SendResponseAsync(response, cancellationToken);
        }
    }

    /// <summary>
    /// Waits for connections.
    /// </summary>
    /// <param name="cancellationToken">Token to stop the task.</param>
    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
        await _pipeServer.WaitForConnectionAsync(cancellationToken);
        
        if (cancellationToken.IsCancellationRequested)
            return;
        
        _currentConnectedUser = GetConnectedUser();
        _logger.LogInformation("Client {User} connected", _currentConnectedUser);
    }
    
    /// <summary>
    /// Disconnects from the current connected client.
    /// </summary>
    private void Disconnect()
    {
        if (!_pipeServer.IsConnected)
        {
            return;
        }

        _pipeServer.Disconnect();
        _logger.LogInformation("Client {User} disconnected", _currentConnectedUser);
        _currentConnectedUser = string.Empty;
    }
    
    /// <summary>
    /// Sends a response to the connected client.
    /// </summary>
    /// <param name="response">The response to be sent to the client.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    private async Task SendResponseAsync(Response response, CancellationToken cancellationToken)
    {
        if (!_pipeServer.IsConnected)
        {
            _logger.LogError("Could not send a response to the client, it is disconnected.");
            return;
        }

        await _messageWriter.WriteAsync(_pipeServer, response, cancellationToken);
    }
    
    /// <summary>
    /// Gets the username of the current connected client.
    /// </summary>
    /// <returns>A string with the name of the user if it's possible, an empty string otherwise.</returns>
    private string GetConnectedUser()
    {
        var user = string.Empty;
        try
        {
            if (_pipeConfig.DisplayImpersonationUser)
                user = _pipeServer.GetImpersonationUserName();
        }
        catch (IOException)
        {
            return user;
        }
        return user;
    }

    private async Task SendStandardOutMessagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (await _stdOutQueueReader.Reader.WaitToReadAsync(cancellationToken))
            {
                string message = await _stdOutQueueReader.Reader.ReadAsync(cancellationToken);
                if (!_pipeServer.IsConnected)
                {
                    _logger.LogDebug("Cannot send standard output message, client is disconnected: {Message}", message);
                    continue;
                }

                await _messageWriter.WriteAsync(_pipeServer, message, cancellationToken);
                _logger.LogDebug("Sent standard output message to client {User}: {Message}", _currentConnectedUser, message);
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }
}
