using System.IO.Pipes;

namespace TiempitoCli.NET.Client;

public class Client
{
    private readonly NamedPipeClientStream _pipeClient;
    private readonly PipeMessageHandler _pipeMessageHandler;
    
    public Client(NamedPipeClientStream pipeClient, PipeMessageHandler pipeMessageHandler)
    {
        _pipeClient = pipeClient;
        _pipeMessageHandler = pipeMessageHandler;
    }
    
    public async Task SendRequestAsync(string message)
    {
        try
        {
            await _pipeClient.ConnectAsync(timeout: 3000); // TODO: Remove hardcoded timeout.
        }
        catch (TimeoutException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Daemon is not running.");
            Console.ForegroundColor = ConsoleColor.White;
            return;
        }

        if (!_pipeClient.IsConnected)
            return;

        await _pipeMessageHandler.WriteAsync(_pipeClient, message);
    }

    public async Task<string> ReceiveResponseAsync()
    {
        if (_pipeClient.IsConnected)
            return await _pipeMessageHandler.ReadAsync(_pipeClient);
        
        return string.Empty;
    }
}
