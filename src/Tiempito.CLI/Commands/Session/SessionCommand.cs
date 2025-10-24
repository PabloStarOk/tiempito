using System.CommandLine;

using Tiempito.CLI.Client.Interfaces;

namespace Tiempito.CLI.Commands.Session;

/// <summary>
/// Represents the root command for managing session-related operations in the CLI.
/// </summary>
internal sealed class SessionCommand : Command
{
    private const string CommandName = "session";
    private const string CommandDescription = "Manage sessions.";

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionCommand"/> class.
    /// </summary>
    /// <param name="commandExecutor">The command executor to use for session commands.</param>
    public SessionCommand(IAsyncCommandExecutor commandExecutor)
        : base(CommandName, CommandDescription)
    {
        var sessionIdOption = new Option<string>("--id", "-i")
        {
            Description = "ID of the session.",
            Arity = ArgumentArity.ExactlyOne,
        };

        var interactiveOption = new Option<bool>("--tty", "-t")
        {
            Description = "Redirects the progress of the session to the current process.",
            Arity = ArgumentArity.ZeroOrOne,
            Required = false,
        };

        var startCommand = new StartSessionCommand(
            commandExecutor,
            Name,
            sessionIdOption,
            interactiveOption);

        var cancelCommand = new GenericSessionCommand(
            commandExecutor,
            Name,
            sessionIdOption,
            "cancel",
            "Cancels a session that is being executed.");

        var pauseCommand = new GenericSessionCommand(
            commandExecutor,
            Name,
            sessionIdOption,
            "pause",
            "Pauses a session that is being executed.");

        var resumeCommand = new GenericSessionCommand(
            commandExecutor,
            Name,
            sessionIdOption,
            "resume",
            "Resumes a paused session.",
            interactiveOption);

        Add(startCommand);
        Add(cancelCommand);
        Add(pauseCommand);
        Add(resumeCommand);
    }
}