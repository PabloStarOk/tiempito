using Microsoft.Extensions.Options;
using System.IO.Pipes;
using Tiempitod.NET.Configuration.Server;
using Tiempitod.NET.Extensions;

namespace Tiempitod.NET.Commands.Server;

public class CommandServer : DaemonService, ICommandServer
{
    private readonly PipeConfig _pipeConfig;
    private readonly NamedPipeServerStream _pipeServer;
    private readonly IAsyncMessageHandler _asyncMessageHandler;
    private CancellationTokenSource _sendMessageTokenSource;
    private CancellationTokenSource _readMessageTokenSource;
    private readonly int _maxRestartAttempts;
    
    public event EventHandler<string> CommandReceived;

    private string _currentConnectedUser = string.Empty;
    private int _currentRestartAttempts;

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
        HandleRequestsAsync().Forget();
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
    
    // TODO: Refactor method
    private async Task HandleRequestsAsync()
    {
        try
        {
            while (!_readMessageTokenSource.IsCancellationRequested)
            {
                // Connect or reconnect the server to a client.
                if (!_pipeServer.IsConnected)
                {
                    await _pipeServer.WaitForConnectionAsync(_readMessageTokenSource.Token);

                    try
                    {
                        if (_pipeConfig.DisplayImpersonationUser)
                            _currentConnectedUser = _pipeServer.GetImpersonationUserName();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "Error when trying to get connected client's username at {Time}", DateTimeOffset.Now);
                    }
                    
                    Logger.LogInformation("Command server connected to client {User}", _currentConnectedUser);
                }
                
                if (!_pipeServer.CanRead)
                {
                    Logger.LogError("Named pipe stream doesn't support read operations.");
                    return;
                }
                
                string receivedCommand = await _asyncMessageHandler.ReadMessageAsync(_pipeServer, _readMessageTokenSource.Token);
                
                // String empty means client has closed its connection.
                if (receivedCommand == string.Empty)
                {
                    _pipeServer.Disconnect();
                    Logger.LogInformation("Command server disconnected from client {User}", _currentConnectedUser);
                    _currentConnectedUser = string.Empty;
                    continue;   
                }
                
                CommandReceived?.Invoke(this, receivedCommand);
            }
        }
        catch (Exception ex)
        {
            if (!_readMessageTokenSource.IsCancellationRequested)
                Logger.LogCritical(ex,"Error while handling command requests at {Time}", DateTimeOffset.Now);
        }
        finally
        {
            if (!_readMessageTokenSource.IsCancellationRequested)
                Restart();
        }
    }

    private void RegenerateToken(ref CancellationTokenSource tokenSource)
    {
        if (tokenSource.TryReset())
            return;

        tokenSource.Dispose();
        tokenSource = new CancellationTokenSource();
    }
}
