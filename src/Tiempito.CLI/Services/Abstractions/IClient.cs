using Tiempito.IPC.Models;

namespace Tiempito.CLI.Services.Abstractions;

/// <summary>
/// Defines a client to connect and send requests to the daemon.
/// </summary>
public interface IClient
{
    public Task SendCommandAsync(Command command, CancellationToken cancellationToken = default);

    public Task<Response?> ReceiveResponseAsync(CancellationToken cancellationToken = default);

    public Task<string> ReadPipeStdInAsync(CancellationToken cancellationToken = default);
}
