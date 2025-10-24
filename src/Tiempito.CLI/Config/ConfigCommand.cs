using System.CommandLine;

using Tiempito.CLI.Client.Interfaces;

namespace Tiempito.CLI.Config;

/// <summary>
/// Provides command and group of commands related to user's configuration.
/// </summary>
public class ConfigCommand
{
    private readonly IAsyncCommandExecutor _asyncCommandExecutor;
    private readonly Option<string> _sessionConfigIdOption;
    private readonly Option<string> _defaultSessionConfigIdOption;
    private readonly Argument<string> _featureArgument;

    /// <summary>
    /// Instantiates a new <see cref="ConfigCommand"/>.
    /// </summary>
    /// <param name="asyncCommandExecutor">An asynchronous executor of commands.</param>
    public ConfigCommand(IAsyncCommandExecutor asyncCommandExecutor)
    {
        _asyncCommandExecutor = asyncCommandExecutor;
        // Set config command
        _defaultSessionConfigIdOption = new Option<string>("--default-config", "-d")
        {
            Description = "Sets the default session configuration to use specifying its ID.",
            Arity = ArgumentArity.ExactlyOne
        };

        // Feature commands
        _featureArgument = new Argument<string>("feature")
        {
            Description = "Feature to enable or disable.",
            Arity = ArgumentArity.ExactlyOne
        };

        // Session config commands
        _sessionConfigIdOption = new Option<string>("--config-id", "-ci")
        {
            Description = "ID of the session configuration.",
            Arity = ArgumentArity.ExactlyOne,
            Required = true
        };
    }

    /// <summary>
    /// Creates a new <see cref="Command"/> named config.
    /// </summary>
    /// <returns>A configured <see cref="Command"/>.</returns>
    public Command GetCommand()
    {
        var configCommand = new Command("config", "Modifies the user's configuration.");

        configCommand.Add(new SetConfigCommand(_asyncCommandExecutor, configCommand.Name, _defaultSessionConfigIdOption));

        configCommand.Add(new GenericFeatureConfigCommand(
            _asyncCommandExecutor, configCommand.Name, _featureArgument,
            "enable", "Enables a specified feature in the user's configuration."));

        configCommand.Add(new GenericFeatureConfigCommand(
            _asyncCommandExecutor, configCommand.Name, _featureArgument,
            "disable", "Disables a specified feature in the user's configuration."));

        configCommand.Add(new CreateSessionConfigCommand(_asyncCommandExecutor, configCommand.Name, _sessionConfigIdOption));
        configCommand.Add(new ModifySessionConfigCommand(_asyncCommandExecutor, configCommand.Name, _sessionConfigIdOption));

        return configCommand;
    }
}
