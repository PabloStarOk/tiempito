using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Models;

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
    public async Task<Response?> SendAsync(
        string commandType,
        string subcommandType,
        IReadOnlyDictionary<string, string> args,
        bool follow = false,
        CancellationToken cancellationToken = default)
    {
        var command = Command.CreateNew(commandType, subcommandType, args, follow);

        try
        {
            await _client.SendCommandAsync(command, cancellationToken);
        }
        catch (TimeoutException)
        {
            return Response.DaemonNotRunning(command.CorrelationId);
        }

        return await _client.ReceiveResponseAsync(cancellationToken);
    }
}