using Tiempito.CLI.Exceptions;
using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Models;
using Tiempito.IPC.Models.Commands;

namespace Tiempito.CLI.Services.Implementations;

/// <summary>
/// Sends commands to the daemon and returns responses.
/// </summary>
internal sealed class CommandSender : ICommandSender
{
    private readonly IClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandSender"/> class.
    /// </summary>
    /// <param name="client">The IPC client used for communication.</param>
    public CommandSender(IClient client)
    {
        _client = client;
    }

    /// <inheritdoc/>
    public async Task<Response> SendAsync(Command command, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.SendMessageAsync(command, cancellationToken);
            return await _client.ReceiveMessageAsync<Response>(useTimeout: true, cancellationToken);
        }
        catch (ResponseTimeoutException)
        {
            return Response.Timeout(
                command.CorrelationId,
                message: "Timed out waiting for the expected response from the daemon.");
        }
        catch (TimeoutException)
        {
            return Response.DaemonNotRunning(command.CorrelationId);
        }
    }
}