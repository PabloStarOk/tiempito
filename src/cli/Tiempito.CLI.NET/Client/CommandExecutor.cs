using Tiempito.IPC.NET.Messages;

namespace Tiempito.CLI.NET.Client;

/// <summary>
/// Represents an executor of commands which sends requests,
/// receive responses and display results to the user.
/// </summary>
public class CommandExecutor : IAsyncCommandExecutor
{
    private readonly IClient _client;
    
    /// <summary>
    /// Instantiates a <see cref="CommandExecutor"/>.
    /// </summary>
    /// <param name="client">Client connection to send and receive response.</param>
    public CommandExecutor(IClient client)
    {
        _client = client;
    }
    
    public async Task ExecuteAsync(string command, string subcommand, IDictionary<string, string> args)
    {
        var request = new Request(command, subcommand, args);
        await _client.SendRequestAsync(request);
        Response response = await _client.ReceiveResponseAsync();
        Console.WriteLine(response.Message); // TODO: Replace with DI to use IConsole.
    }
}
