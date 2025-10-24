using System.CommandLine;

using Tiempito.CLI.Client.Interfaces;
using Tiempito.CLI.Config;

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
    /// <param name="commandExecutor">The executor responsible for running asynchronous commands.</param>
    public ConfigCommand(IAsyncCommandExecutor commandExecutor)
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

        var configCommand = new Command(CommandName, CommandDescription);

        var createSessionConfigCommand = new CreateSessionConfigCommand(
            commandExecutor,
            configCommand.Name,
            sessionConfigIdOption);

        var modifySessionConfigCommand = new ModifySessionConfigCommand(
            commandExecutor,
            configCommand.Name,
            sessionConfigIdOption);

        var setCommand = new SetConfigCommand(
            commandExecutor,
            configCommand.Name,
            defaultSessionConfigIdOption);

        var enableFeatCommand = new GenericFeatureConfigCommand(
            commandExecutor,
            configCommand.Name,
            featureArgument,
            "enable",
            "Enables a specified feature in the user's configuration.");

        var disableFeatCommand = new GenericFeatureConfigCommand(
            commandExecutor,
            configCommand.Name,
            featureArgument,
            "disable",
            "Disables a specified feature in the user's configuration.");

        Add(setCommand);
        Add(enableFeatCommand);
        Add(disableFeatCommand);
        Add(createSessionConfigCommand);
        Add(modifySessionConfigCommand);
    }
}