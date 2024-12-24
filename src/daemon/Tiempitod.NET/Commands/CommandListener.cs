using System.IO.Pipes;
using System.Text;

namespace Tiempitod.NET.Commands;

public class CommandListener : DaemonService, ICommandListener
{
    private readonly NamedPipeServerStream _pipeServer;
    private readonly Encoding _streamEncoding;
    private CancellationTokenSource _serverTokenSource;
    private readonly int _maxRestartAttempts = 3;
    private int _currentRestartAttempts;
    private Task? _executePipeAsync;

    public event EventHandler<string> CommandReceived;
    
    public CommandListener(ILogger<CommandListener> logger, Encoding streamEncoding) : base (logger)
    {
        // TODO: Use dependency injection to instantiate pipe.
        _pipeServer = new NamedPipeServerStream("tiempito-pipe", PipeDirection.In, 1);
        _streamEncoding = streamEncoding;
        _serverTokenSource = new CancellationTokenSource();
    }

    protected override void OnStartService()
    {
        Start();
    }

    protected override void OnStopService()
    {
        StopAsync().GetAwaiter().GetResult();
    }

    public void Start()
    {
        if (_serverTokenSource.IsCancellationRequested && !_serverTokenSource.TryReset())
        {
            _serverTokenSource.Dispose();
            _serverTokenSource = new CancellationTokenSource();
        }
        
        _executePipeAsync = HandleRequestsAsync(_serverTokenSource.Token);
        Logger.LogInformation("Command listener started.");
    }

    public void Restart()
    {
        if (_maxRestartAttempts > 0 && _currentRestartAttempts > _maxRestartAttempts)
            return;
        
        _currentRestartAttempts++;
            
        if (_pipeServer.IsConnected)
            _pipeServer.Disconnect();
        
        Start();
        Logger.LogInformation("Command listener restarted.");
    }
    
    public async Task StopAsync()
    {
        await _serverTokenSource.CancelAsync();
        
        if (_pipeServer.IsConnected)
            _pipeServer.Disconnect();
        
        await _pipeServer.DisposeAsync();
        
        Logger.LogInformation("Command listener stopped.");
    }
    
    private async Task HandleRequestsAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Connect or reconnect the server to a client.
                if (!_pipeServer.IsConnected)
                {
                    await _pipeServer.WaitForConnectionAsync(stoppingToken);
                    Logger.LogInformation("Command listener connected to client.");
                }
                
                // Read length of the buffer. (Sender must append length of the buffer in the first two bytes)
                int length = _pipeServer.ReadByte() * 256;
                length += _pipeServer.ReadByte();
                
                if (length < 0)
                {
                    _pipeServer.Disconnect();
                    Logger.LogInformation("Command listener disconnected from client.");
                    continue;
                }
                
                // Read command request.
                var dataBuffer = new Memory<byte>(new byte[length]);
                
                if (await _pipeServer.ReadAsync(dataBuffer, stoppingToken) <= 0)
                    continue;
                
                string receivedCommand = _streamEncoding.GetString(dataBuffer.ToArray());
                CommandReceived?.Invoke(this, receivedCommand);
            }
        }
        catch (Exception ex)
        {
            if (!stoppingToken.IsCancellationRequested)
                Logger.LogCritical("Error while handling command requests at {time}: {error}", DateTimeOffset.Now, ex.Message);
        }
        finally
        {
            if (!stoppingToken.IsCancellationRequested)
                Restart();
        }
    }
}
