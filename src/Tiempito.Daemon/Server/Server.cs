using System.IO.Pipes;
using AsyncEvent;
using Microsoft.Extensions.Options;

using Tiempito.Daemon.Application.Commands;
using Tiempito.Daemon.Application.Notifications;
using Tiempito.Daemon.Server.Configuration;
using Tiempito.Daemon.Server.Extensions;
using Tiempito.IPC.Abstractions;
using Tiempito.IPC.Models;

namespace Tiempito.Daemon.Server;

/// <summary>
/// Represents the server to receive requests and send responses to the client.
/// </summary>
public class Server : IServer
{
    private readonly ILogger<Server> _logger;
    private readonly PipeConfig _pipeConfig;
    private readonly NamedPipeServerStream _pipeServer;
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IStandardOutQueueReader _stdOutQueueReader;
    private readonly IMessageWriter _messageWriter;
    private readonly IMessageReader _messageReader;
    private readonly int _maxRestartAttempts;
    private string _currentConnectedUser = string.Empty;
    private int _currentRestartAttempts;
    private Task? _stdOutMessagesSendTask;

    public event AsyncEventHandler? OnFailed;

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
        _maxRestartAttempts = daemonConfigOptions.Value.MaxRestartAttempts;
        _messageWriter = messageWriter;
        _messageReader = messageReader;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(() => RunAsync(cancellationToken), cancellationToken).Forget();
        _stdOutMessagesSendTask = SendStandardOutMessagesAsync(cancellationToken);
        _logger.LogInformation("Server started");
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (_pipeServer.IsConnected)
            await DisconnectAsync();
        
        if (_stdOutMessagesSendTask is not null)
        {
            await _stdOutMessagesSendTask;
            _stdOutMessagesSendTask.Dispose();
        }

        await _pipeServer.DisposeAsync();
        
        _logger.LogInformation("Server stopped");
    }

    /// <summary>
    /// Restarts the server.
    /// </summary>
    private async Task RestartAsync(CancellationToken cancellationToken)
    {
        _currentRestartAttempts++;
        if (_maxRestartAttempts > 0 && _currentRestartAttempts > _maxRestartAttempts)
        {
            _logger.LogError("Maximum restart attempts reached, command server will not restart.");
            if (OnFailed is not null)
                await OnFailed.InvokeAsync(this, EventArgs.Empty);
        }
        
        if (_pipeServer.IsConnected)
            _pipeServer.Disconnect();
        
        Task.Run(() => RunAsync(cancellationToken), cancellationToken).Forget();
        _logger.LogCritical("Command server restarted.");
    }

    /// <summary>
    /// Runs the server to connect and disconnect from the client and handle requests.
    /// </summary>
    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_pipeServer.IsConnected)
                    await ConnectAsync(cancellationToken);
                
                // Handle client requests
                Command? command = await ReceiveRequestsAsync(cancellationToken);

                if (command is null) // TODO: Replace with a termination request.
                {
                    await DisconnectAsync();
                    continue;
                }

                Response response = await _commandDispatcher.DispatchAsync(command, cancellationToken);
                await SendResponseAsync(response, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogCritical(ex,"Error while running command server at {Time}", DateTimeOffset.Now);
                await RestartAsync(cancellationToken);
            }
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
        _logger.LogInformation("Command server connected to client {User}", _currentConnectedUser);
    }
    
    /// <summary>
    /// Disconnects from the current connected client.
    /// </summary>
    private async Task DisconnectAsync()
    {
        _pipeServer.Disconnect();
        _logger.LogInformation("Command server disconnected from client {User}", _currentConnectedUser);
        _currentConnectedUser = string.Empty;
    }

    /// <summary>
    /// Receives all incoming requests from the current connected client.
    /// </summary>
    /// <param name="cancellationToken">Token to stop the task.</param>
    /// <returns>A string with the received message.</returns>
    private async Task<Command?> ReceiveRequestsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!_pipeServer.IsConnected)
                break;
            
            if (!_pipeServer.CanRead)
                _logger.LogError("Named pipe stream doesn't support read operations.");

            return await _messageReader.ReadAsync<Command>(_pipeServer, cancellationToken);
        }

        return null;
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
        
        if (!_pipeServer.CanWrite)
        {
            _logger.LogError("Named pipe stream does not support write operations.");
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
        catch
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
