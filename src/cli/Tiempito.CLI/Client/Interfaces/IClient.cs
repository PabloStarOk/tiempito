using Tiempito.IPC.Models;

namespace Tiempito.CLI.Client.Interfaces;

/// <summary>
/// Defines a client to connect and send requests to the daemon.
/// </summary>
public interface IClient
{
    public Task SendRequestAsync(Request request, CancellationToken cancellationToken = default);

    public Task<Response?> ReceiveResponseAsync(CancellationToken cancellationToken = default);
    
    public Task<string> ReadPipeStdInAsync(CancellationToken cancellationToken = default);
}
