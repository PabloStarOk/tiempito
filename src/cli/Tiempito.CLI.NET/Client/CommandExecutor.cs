using Tiempito.IPC.NET.Messages;

namespace Tiempito.CLI.NET.Client;

/// <summary>
/// Represents an executor of commands which sends requests,
/// receive responses and display results to the user.
/// </summary>
public class CommandExecutor : IAsyncCommandExecutor
{
    private readonly IClient _client;
    private readonly TextWriter _stdOut;
    private readonly TextWriter _stdErr;
    
    /// <summary>
    /// Instantiates a <see cref="CommandExecutor"/>.
    /// </summary>
    /// <param name="client">Client connection to send and receive response.</param>
    /// <param name="stdOut">Current standard output to display a response to the user.</param>
    /// <param name="stdErr">Current standard error to display any error to the user.</param>
    public CommandExecutor(IClient client, TextWriter stdOut, TextWriter stdErr)
    {
        _client = client;
        _stdOut = stdOut;
        _stdErr = stdErr;
    }
    
    public async Task ExecuteAsync(string command, string subcommand, IReadOnlyDictionary<string, string> args)
    {
        // Send request.
        try
        {
            var request = new Request(command, subcommand, args);
            await _client.SendRequestAsync(request);
        }
        catch (TimeoutException)
        {
            await _stdErr.WriteLineAsync("Daemon is not running.");
            return;
        }
        catch
        {
            await _stdErr.WriteLineAsync("An error occurred when trying to send a request.");
        }

        // Receive response.
        try
        {
            Response response = await _client.ReceiveResponseAsync();
            await _stdOut.WriteLineAsync(response.Message);
        }
        catch
        {
            await _stdErr.WriteLineAsync("An error occurred when trying to receive a response.");
        }
    }
}
