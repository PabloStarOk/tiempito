using System.CommandLine;

using Tiempito.CLI.Services.Abstractions;

namespace Tiempito.CLI.Commands.Config;

/// <summary>
/// Represents the root command for managing user configuration in the CLI.
/// </summary>
internal sealed class ConfigCommand : Command
{
    private const string CommandName = "config";
    private const string CommandDescription = "Manage user's configuration.";

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigCommand"/> class.
    /// </summary>
    /// <param name="commandSender">The sender used to execute session commands.</param>
    /// <param name="messageWriter">The writer used to output messages to the terminal.</param>
    public ConfigCommand(ICommandSender commandSender, IMessageWriter messageWriter)
        : base(CommandName, CommandDescription)
    {
        var defaultSessionConfigIdOption = new Option<string>("--default-config", "-d")
        {
            Description = "Sets the default session configuration to use specifying its ID.",
            Arity = ArgumentArity.ExactlyOne,
        };

        var featureArgument = new Argument<string>("feature")
        {
            Description = "Feature to enable or disable.",
            Arity = ArgumentArity.ExactlyOne,
        };

        var sessionConfigIdOption = new Option<string>("--config-id", "-ci")
        {
            Description = "ID of the session configuration.",
            Arity = ArgumentArity.ExactlyOne,
            Required = true,
        };

        var createSessionConfigCommand = new CreateSessionConfigCommand(
            commandSender,
            messageWriter,
            sessionConfigIdOption);

        var modifySessionConfigCommand = new ModifySessionConfigCommand(
            commandSender,
            messageWriter,
            sessionConfigIdOption);

        var setCommand = new SetConfigCommand(
            commandSender,
            messageWriter,
            defaultSessionConfigIdOption);

        var enableFeatCommand = new UserFeatureConfigCommand(
            commandSender,
            messageWriter,
            featureArgument,
            "enable",
            "Enables a specified feature in the user's configuration.",
            enable: true);

        var disableFeatCommand = new UserFeatureConfigCommand(
            commandSender,
            messageWriter,
            featureArgument,
            "disable",
            "Disables a specified feature in the user's configuration.",
            enable: false);

        Add(setCommand);
        Add(enableFeatCommand);
        Add(disableFeatCommand);
        Add(createSessionConfigCommand);
        Add(modifySessionConfigCommand);
    }
}