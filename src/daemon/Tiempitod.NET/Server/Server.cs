using Microsoft.Extensions.Options;
using System.IO.Pipes;
using Tiempito.IPC.NET.Messages;
using Tiempito.IPC.NET.Packets;
using Tiempitod.NET.Configuration.Server;
using Tiempitod.NET.Extensions;

namespace Tiempitod.NET.Server; 

/// <summary>
/// Represents the server to receive requests and send responses to the client.
/// </summary>
public class Server : DaemonService, IServer
{
    private readonly PipeConfig _pipeConfig;
    private readonly NamedPipeServerStream _pipeServer;
    private readonly IAsyncPacketHandler _asyncPacketHandler;
    private readonly IPacketSerializer _packetSerializer;
    private readonly IPacketDeserializer _packetDeserializer;
    private readonly IStandardOutSink _stdOutSink;
    private CancellationTokenSource _sendMessageTokenSource;
    private CancellationTokenSource _readMessageTokenSource;
    private readonly int _maxRestartAttempts;
    private string _currentConnectedUser = string.Empty;
    private int _currentRestartAttempts;
    
    public event EventHandler<Request>? RequestReceived;

    public Server(
        ILogger<Server> logger,
        IOptions<PipeConfig> daemonConfigOptions,
        NamedPipeServerStream pipeServer,
        IStandardOutSink stdOutSink,
        IAsyncPacketHandler asyncPacketHandler,
        IPacketSerializer packetSerializer,
        IPacketDeserializer packetDeserializer) : base(logger)
    {
        _pipeConfig = daemonConfigOptions.Value;
        _pipeServer = pipeServer;
        _stdOutSink = stdOutSink;
        _asyncPacketHandler = asyncPacketHandler;
        _packetSerializer = packetSerializer;
        _packetDeserializer = packetDeserializer;
        _maxRestartAttempts = daemonConfigOptions.Value.MaxRestartAttempts;
        
        _sendMessageTokenSource = new CancellationTokenSource();
        _readMessageTokenSource = new CancellationTokenSource();
    }

    public async Task SendResponseAsync(Response response)
    {
        if (!_pipeServer.IsConnected)
        {
            Logger.LogError("Could not send a response to the client, it is disconnected.");
            return;
        }
        
        if (!_pipeServer.CanWrite)
        {
            Logger.LogError("Named pipe stream doesn't support write operations.");
            return;
        }

        Packet outgoingPacket = _packetSerializer.Serialize(response);
        await _asyncPacketHandler.WritePacketAsync(_pipeServer, outgoingPacket, _sendMessageTokenSource.Token);
    }
    
    protected override void OnStartService()
    {
        Start();
        Logger.LogInformation("Command server started.");
    }

    protected override void OnStopService()
    {
        StopAsync().GetAwaiter().GetResult();
        Logger.LogInformation("Command server restarted.");
    }

    /// <summary>
    /// Starts the server.
    /// </summary>
    private void Start()
    {
        _readMessageTokenSource = RegenerateTokenSource(_readMessageTokenSource);
        _sendMessageTokenSource = RegenerateTokenSource(_sendMessageTokenSource);
        Task.Run(() => RunAsync(_readMessageTokenSource.Token)).Forget();
    }

    /// <summary>
    /// Restarts the server.
    /// </summary>
    private void Restart()
    {
        if (_maxRestartAttempts > 0 && _currentRestartAttempts > _maxRestartAttempts)
        {
            Logger.LogError("Maximum restart attempts reached, command server will not restart.");
            return;
        }
        _currentRestartAttempts++;
        
        if (_pipeServer.IsConnected)
            _pipeServer.Disconnect();
        
        Start();
        Logger.LogWarning("Command server restarted.");
    }
    
    /// <summary>
    /// Stops the server.
    /// </summary>
    private async Task StopAsync()
    {
        await _readMessageTokenSource.CancelAsync();
        await _sendMessageTokenSource.CancelAsync();
        
        _sendMessageTokenSource.Dispose();
        _readMessageTokenSource.Dispose();
        
        if (_pipeServer.IsConnected)
            _pipeServer.Disconnect();
        
        await _pipeServer.DisposeAsync();
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
                RequestReceived?.Invoke(this, request);
                
                if (request.RedirectProgress)
                    _stdOutSink.Start(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            if (!_readMessageTokenSource.IsCancellationRequested)
                Logger.LogCritical(ex,"Error while running command server at {Time}", DateTimeOffset.Now);
            
            if (!_readMessageTokenSource.IsCancellationRequested)
                Restart();
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
        Logger.LogInformation("Command server connected to client {User}", _currentConnectedUser);
    }
    
    /// <summary>
    /// Disconnects from the current connected client.
    /// </summary>
    private async Task DisconnectAsync()
    {
        await _stdOutSink.StopAsync();
        _pipeServer.Disconnect();
        Logger.LogInformation("Command server disconnected from client {User}", _currentConnectedUser);
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

            Logger.LogError("Named pipe stream doesn't support read operations.");
        }

        return new Packet(string.Empty.Length, string.Empty);
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
