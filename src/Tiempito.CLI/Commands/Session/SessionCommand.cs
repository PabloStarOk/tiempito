using System.CommandLine;

using Tiempito.CLI.Services.Abstractions;

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
    /// <param name="commandSender">The sender used to execute session commands.</param>
    /// <param name="messageWriter">The writer used to output messages to the terminal.</param>
    /// <param name="sessionFollower">The follower used to track session progress.</param>
    public SessionCommand(
        ICommandSender commandSender,
        IMessageWriter messageWriter,
        ISessionFollower sessionFollower)
        : base(CommandName, CommandDescription)
    {
        var sessionIdOption = new Option<string>("--id", "-i")
        {
            Description = "ID of the session.",
            Arity = ArgumentArity.ExactlyOne,
        };

        var followOption = new Option<bool>("--follow", "-f")
        {
            Description = "Whether to follow session progress.",
            Arity = ArgumentArity.ZeroOrOne,
            Required = false,
        };

        var startCommand = new StartSessionCommand(
            commandSender,
            messageWriter,
            sessionFollower,
            Name,
            sessionIdOption,
            followOption);

        var cancelCommand = new GenericSessionCommand(
            commandSender,
            messageWriter,
            sessionFollower,
            Name,
            sessionIdOption,
            "cancel",
            "Cancels a session that is being executed.");

        var pauseCommand = new GenericSessionCommand(
            commandSender,
            messageWriter,
            sessionFollower,
            Name,
            sessionIdOption,
            "pause",
            "Pauses a session that is being executed.");

        var resumeCommand = new GenericSessionCommand(
            commandSender,
            messageWriter,
            sessionFollower,
            Name,
            sessionIdOption,
            "resume",
            "Resumes a paused session.",
            followOption);

        Add(startCommand);
        Add(cancelCommand);
        Add(pauseCommand);
        Add(resumeCommand);
    }
}