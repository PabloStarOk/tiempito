using System.IO.Pipes;
using AsyncEvent;
using Microsoft.Extensions.Options;

using Tiempito.Daemon.Application.Commands;
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
    private readonly IStandardOutSink _stdOutSink;
    private readonly ICommandHandler _commandHandler;
    private readonly IMessageWriter _messageWriter;
    private readonly IMessageReader _messageReader;
    private readonly int _maxRestartAttempts;
    private string _currentConnectedUser = string.Empty;
    private int _currentRestartAttempts;

    public event AsyncEventHandler? OnFailed;

    public Server(
        ILogger<Server> logger,
        IOptions<PipeConfig> daemonConfigOptions,
        NamedPipeServerStream pipeServer,
        IStandardOutSink stdOutSink,
        ICommandHandler commandHandler,
        IMessageWriter messageWriter,
        IMessageReader messageReader)
    {
        _logger = logger;
        _pipeConfig = daemonConfigOptions.Value;
        _pipeServer = pipeServer;
        _stdOutSink = stdOutSink;
        _commandHandler = commandHandler;
        _maxRestartAttempts = daemonConfigOptions.Value.MaxRestartAttempts;
        _messageWriter = messageWriter;
        _messageReader = messageReader;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(() => RunAsync(cancellationToken), cancellationToken).Forget();
        _logger.LogInformation("Server started");
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (_pipeServer.IsConnected)
            await DisconnectAsync();
        
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
                Command? incomingRequest = await ReceiveRequestsAsync(cancellationToken);

                if (incomingRequest is null) // TODO: Replace with a termination request.
                {
                    await DisconnectAsync();
                    continue;
                }

                Response response = await _commandHandler.HandleAsync(incomingRequest, cancellationToken);
                await SendResponseAsync(response, cancellationToken);
                
                if (incomingRequest.RedirectProgress)
                    _stdOutSink.Start(cancellationToken);
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
        await _stdOutSink.StopAsync();
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
}
