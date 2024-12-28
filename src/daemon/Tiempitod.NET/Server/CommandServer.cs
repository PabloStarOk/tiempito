using Microsoft.Extensions.Options;
using System.IO.Pipes;
using Tiempitod.NET.Configuration.Server;
using Tiempitod.NET.Extensions;
using Tiempitod.NET.Server.Messages;

namespace Tiempitod.NET.Server;

public class CommandServer : DaemonService, ICommandServer
{
    private readonly PipeConfig _pipeConfig;
    private readonly NamedPipeServerStream _pipeServer;
    private readonly IAsyncMessageHandler _asyncMessageHandler;
    private CancellationTokenSource _sendMessageTokenSource;
    private CancellationTokenSource _readMessageTokenSource;
    private readonly int _maxRestartAttempts;
    private string _currentConnectedUser = string.Empty;
    private int _currentRestartAttempts;
    public event EventHandler<string> CommandReceived;

    public CommandServer(
        ILogger<CommandServer> logger,
        IOptions<PipeConfig> daemonConfigOptions,
        NamedPipeServerStream pipeServer,
        IAsyncMessageHandler asyncMessageHandler) : base(logger)
    {
        _pipeConfig = daemonConfigOptions.Value;
        _pipeServer = pipeServer;
        _asyncMessageHandler = asyncMessageHandler;
        _maxRestartAttempts = daemonConfigOptions.Value.MaxRestartAttempts;
        
        _sendMessageTokenSource = new CancellationTokenSource();
        _readMessageTokenSource = new CancellationTokenSource();
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

    public void Start()
    {
        RegenerateToken(ref _readMessageTokenSource);
        RegenerateToken(ref _sendMessageTokenSource);
        Task.Run(() => RunAsync(_readMessageTokenSource.Token)).Forget();
    }

    public void Restart()
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
    
    public async Task StopAsync()
    {
        await _readMessageTokenSource.CancelAsync();
        await _sendMessageTokenSource.CancelAsync();
        
        _sendMessageTokenSource.Dispose();
        _readMessageTokenSource.Dispose();
        
        if (_pipeServer.IsConnected)
            _pipeServer.Disconnect();
        
        await _pipeServer.DisposeAsync();
    }

    public async Task SendResponseAsync(DaemonResponse response)
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
        
        await _asyncMessageHandler.SendMessageAsync(_pipeServer, response, _sendMessageTokenSource.Token);
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
                string commandReceived = await ReceiveRequestsAsync(cancellationToken);
                
                // Empty means disconnection.
                if (string.IsNullOrEmpty(commandReceived))
                    Disconnect();
                
                CommandReceived?.Invoke(this, commandReceived);
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
    private void Disconnect()
    {
        _pipeServer.Disconnect();
        Logger.LogInformation("Command server disconnected from client {User}", _currentConnectedUser);
        _currentConnectedUser = string.Empty;
    }

    /// <summary>
    /// Receives all incoming requests from the current connected client.
    /// </summary>
    /// <param name="cancellationToken">Token to stop the task.</param>
    /// <returns>A string with the received message.</returns>
    private async Task<string> ReceiveRequestsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!_pipeServer.IsConnected)
                break;
            
            if (_pipeServer.CanRead)
                return await _asyncMessageHandler.ReadMessageAsync(_pipeServer, cancellationToken);

            Logger.LogError("Named pipe stream doesn't support read operations.");
        }
        return string.Empty;
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
    
    /// <summary>
    /// Regenerates the given token
    /// </summary>
    /// <param name="tokenSource"></param>
    private static void RegenerateToken(ref CancellationTokenSource tokenSource)
    {
        if (tokenSource.TryReset())
            return;

        tokenSource.Dispose();
        tokenSource = new CancellationTokenSource();
    }
}
