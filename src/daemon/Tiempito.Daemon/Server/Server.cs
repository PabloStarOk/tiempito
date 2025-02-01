using Microsoft.Extensions.Options;
using System.IO.Pipes;

using AsyncEvent;

using Tiempito.Daemon.Configuration.Daemon.Objects;
using Tiempito.IPC.NET.Messages;
using Tiempito.IPC.NET.Packets;

using Tiempito.Daemon.Common.Extensions;
using Tiempito.Daemon.Server.Requests;
using Tiempito.Daemon.Server.StandardOut;

namespace Tiempito.Daemon.Server; 

/// <summary>
/// Represents the server to receive requests and send responses to the client.
/// </summary>
public class Server : IServer
{
    private readonly ILogger<Server> _logger;
    private readonly PipeConfig _pipeConfig;
    private readonly NamedPipeServerStream _pipeServer;
    private readonly IAsyncPacketHandler _asyncPacketHandler;
    private readonly IPacketSerializer _packetSerializer;
    private readonly IPacketDeserializer _packetDeserializer;
    private readonly IStandardOutSink _stdOutSink;
    private readonly IRequestHandler _requestHandler;
    private readonly int _maxRestartAttempts;
    private string _currentConnectedUser = string.Empty;
    private int _currentRestartAttempts;

    public event AsyncEventHandler? OnFailed;
    
    public Server(
        ILogger<Server> logger,
        IOptions<PipeConfig> daemonConfigOptions,
        NamedPipeServerStream pipeServer,
        IStandardOutSink stdOutSink,
        IAsyncPacketHandler asyncPacketHandler,
        IPacketSerializer packetSerializer,
        IPacketDeserializer packetDeserializer,
        IRequestHandler requestHandler)
    {
        _logger = logger;
        _pipeConfig = daemonConfigOptions.Value;
        _pipeServer = pipeServer;
        _stdOutSink = stdOutSink;
        _asyncPacketHandler = asyncPacketHandler;
        _packetSerializer = packetSerializer;
        _packetDeserializer = packetDeserializer;
        _requestHandler = requestHandler;
        _maxRestartAttempts = daemonConfigOptions.Value.MaxRestartAttempts;
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
                Packet incomingPacket = await ReceiveRequestsAsync(cancellationToken);
                
                // A length lower than zero means a client disconnection.
                if (incomingPacket.Length < 0) // TODO: Replace with a termination request.
                {
                    await DisconnectAsync();
                    continue;
                }

                var request = _packetDeserializer.Deserialize<Request>(incomingPacket);
                Response response = await _requestHandler.HandleAsync(request, cancellationToken);
                await SendResponseAsync(response, cancellationToken);
                
                if (request.RedirectProgress)
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
    private async Task<Packet> ReceiveRequestsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!_pipeServer.IsConnected)
                break;
            
            if (_pipeServer.CanRead)
                return await _asyncPacketHandler.ReadPacketAsync(_pipeServer, cancellationToken);

            _logger.LogError("Named pipe stream doesn't support read operations.");
        }

        return new Packet(string.Empty.Length, string.Empty);
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
            _logger.LogError("Named pipe stream doesn't support write operations.");
            return;
        }

        Packet outgoingPacket = _packetSerializer.Serialize(response);
        await _asyncPacketHandler.WritePacketAsync(_pipeServer, outgoingPacket, cancellationToken);
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
