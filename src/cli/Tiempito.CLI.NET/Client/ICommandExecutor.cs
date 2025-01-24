namespace Tiempito.CLI.NET.Client;

/// <summary>
/// Defines an executor of incoming commands.
/// </summary>
public interface IAsyncCommandExecutor
{
    /// <summary>
    /// Sends a request to the daemon to execute a command and
    /// receives the response and displays the result to the user.
    /// </summary>
    /// <param name="command">Command parent.</param>
    /// <param name="subcommand">Subcommand of the parent.</param>
    /// <param name="args">Arguments of the command.</param>
    /// <param name="tty">If the server's named pipe output is redirected to the process.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ExecuteAsync(string command, string subcommand, IReadOnlyDictionary<string, string> args, bool tty = false);
}
