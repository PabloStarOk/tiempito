using Tiempito.CLI.Client.Interfaces;
using Tiempito.IPC.Enums;
using Tiempito.IPC.Models;

namespace Tiempito.CLI.Client;

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
    /// Initializes a new instance of the <see cref="CommandExecutor"/> class.
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

    /// <inheritdoc/>
    public async Task ExecuteAsync(string command, string subcommand, IReadOnlyDictionary<string, string> args, bool follow = false)
    {
        // Send request.
        try
        {
            var request = Command.CreateNew(command, subcommand, args, follow);
            await _client.SendRequestAsync(request);
        }
        catch (TimeoutException)
        {
            await _stdErr.WriteLineAsync("Daemon is not running.");
            return;
        }

        // Receive response.
        try
        {
            Response? response = await _client.ReceiveResponseAsync();

            switch (response?.StatusCode)
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

            if (follow)
                Console.CancelKeyPress += (_, _) => follow = false;
            while (follow)
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
