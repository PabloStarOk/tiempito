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

    public async Task ExecuteAsync(string command, string subcommand, IReadOnlyDictionary<string, string> args, bool tty = false)
    {
        // Send request.
        try
        {
            var request = new Request(command, subcommand, args, tty);
            await _client.SendRequestAsync(request);
        }
        catch (TimeoutException)
        {
            await _stdErr.WriteLineAsync("Daemon is not running.");
            return;
        }
        catch (Exception ex)
        {
            await _stdErr.WriteLineAsync($"An error occurred when trying to send a request. Error: {ex.Message}");
        }

        // Receive response.
        try
        {
            Response response = await _client.ReceiveResponseAsync();
            
            switch (response.StatusCode)
            {
                case ResponseStatusCode.Ok:
                    await _stdOut.WriteLineAsync(response.Message);
                    break;
                    
                case ResponseStatusCode.BadRequest:
                case ResponseStatusCode.Error:
                    await _stdErr.WriteLineAsync(response.Message);
                    return;
                
                default:
                    throw new InvalidOperationException("Response status code unrecognized.");
            }
            
            if (tty)
                Console.CancelKeyPress += (_, _) => tty = false;
            while (tty)
            {
                string message = await _client.ReadPipeStdInAsync();
                await _stdOut.WriteLineAsync(message);
            }
        }
        catch (Exception ex)
        {
            await _stdErr.WriteLineAsync($"An error occurred when trying to receive a response. Error: {ex.Message}");
        }
    }
}
