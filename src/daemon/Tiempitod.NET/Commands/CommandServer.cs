using System.IO.Pipes;
using System.Text;

namespace Tiempitod.NET.Commands;

public class CommandServer : ICommandServer
{
    private readonly NamedPipeServerStream _pipeServer;
    private readonly Encoding _streamEncoding;
    private readonly ILogger<CommandServer> _logger;
    private CancellationTokenSource _serverTokenSource;
    private readonly int _maxRestartAttempts = 3;
    private int _currentRestartAttempts;
    private Task? _executePipeAsync;
    private bool _isDisposed = false;

    public event EventHandler<string> CommandReceived;
    
    public CommandServer(ILogger<CommandServer> logger, Encoding streamEncoding)
    {
        // TODO: Use dependency injection to instantiate pipe.
        _pipeServer = new NamedPipeServerStream("tiempito-pipe", PipeDirection.In, 1);
        _logger = logger;
        _streamEncoding = streamEncoding;
        _serverTokenSource = new CancellationTokenSource();
    }
    
    public void Start()
    {
        if (_serverTokenSource.IsCancellationRequested && !_serverTokenSource.TryReset())
        {
            _serverTokenSource.Dispose();
            _serverTokenSource = new CancellationTokenSource();
        }
        
        _executePipeAsync = HandleRequestsAsync(_serverTokenSource.Token);
        _logger.LogInformation("Command server started.");
    }

    public void Restart()
    {
        if (_maxRestartAttempts > 0 && _currentRestartAttempts > _maxRestartAttempts)
            return;
        
        _currentRestartAttempts++;
            
        if (_pipeServer.IsConnected)
            _pipeServer.Disconnect();
        
        Start();
        _logger.LogInformation("Command server restarted.");
    }
    
    public async Task StopAsync()
    {
        await _serverTokenSource.CancelAsync();
        await _pipeServer.DisposeAsync();
        _logger.LogInformation("Command server stopped.");
        Dispose();
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
                    _logger.LogInformation("Command server connected to client.");
                }
                
                // Read length of the buffer. (Sender must append length of the buffer in the first two bytes)
                int length = _pipeServer.ReadByte() * 256;
                length += _pipeServer.ReadByte();
                
                if (length < 0)
                {
                    _pipeServer.Disconnect();
                    _logger.LogInformation("Command server disconnected from client.");
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
                _logger.LogCritical("Error while handling command requests at {time}: {error}", DateTimeOffset.Now, ex.Message);
        }
        finally
        {
            if (!stoppingToken.IsCancellationRequested)
                Restart();
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;
        
        if (disposing) {
            if (_pipeServer.IsConnected)
                _pipeServer.Disconnect();
            
            _pipeServer.Dispose();
        }

        _isDisposed = true;
    }

    ~CommandServer()
    {
        Dispose(disposing: false);
    }
}
