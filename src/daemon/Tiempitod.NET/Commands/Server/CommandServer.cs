using System.IO.Pipes;

namespace Tiempitod.NET.Commands.Server;

public class CommandServer : DaemonService, ICommandServer
{
    private readonly NamedPipeServerStream _pipeServer;
    private readonly IAsyncMessageHandler _asyncMessageHandler;
    private CancellationTokenSource _sendMessageTokenSource;
    private CancellationTokenSource _readMessageTokenSource;
    
    private string _currentConnectedUser = string.Empty;
    private readonly int _maxRestartAttempts = 3;
    private int _currentRestartAttempts;

    public event EventHandler<string> CommandReceived;

    public CommandServer(ILogger<CommandServer> logger, IAsyncMessageHandler asyncMessageHandler) : base(logger)
    {
        // TODO: Use dependency injection to instantiate pipe.
        _pipeServer = new NamedPipeServerStream("tiempito-pipe", PipeDirection.InOut, 1);
        _asyncMessageHandler = asyncMessageHandler;
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
                    //_currentConnectedUser = _pipeServer.GetImpersonationUserName();
                    Logger.LogInformation("Command server connected to client");
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
                    Logger.LogInformation("Command server disconnected from client");
                    _currentConnectedUser = string.Empty;
                    continue;   
                }
                
                CommandReceived?.Invoke(this, receivedCommand);
            }
        }
        catch (Exception ex)
        {
            if (!_readMessageTokenSource.IsCancellationRequested)
                Logger.LogCritical("Error while handling command requests at {time}: {error}", DateTimeOffset.Now, ex.Message);
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
