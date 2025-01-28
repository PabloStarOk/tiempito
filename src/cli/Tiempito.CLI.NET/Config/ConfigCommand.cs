using System.CommandLine;
using Tiempito.CLI.NET.Client;

namespace Tiempito.CLI.NET.Config;

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
        _defaultSessionConfigIdOption = new Option<string>("--default-config", "Sets the default session configuration to use specifying its ID.")
        {
            Arity = ArgumentArity.ExactlyOne
        };
        _defaultSessionConfigIdOption.AddAlias("-d");
        
        // Feature commands
        _featureArgument = new Argument<string>("feature", "Feature to enable or disable.")
        {
            Arity = ArgumentArity.ExactlyOne
        };
        
        // Session config commands
        _sessionConfigIdOption = new Option<string>("--config-id", "ID of the session configuration.")
        {
            Arity = ArgumentArity.ExactlyOne,
            IsRequired = true
        };
        _sessionConfigIdOption.AddAlias("-ci");
    }

    /// <summary>
    /// Creates a new <see cref="Command"/> named config.
    /// </summary>
    /// <returns>A configured <see cref="Command"/>.</returns>
    public Command GetCommand()
    {
        var configCommand = new Command("config", "Modifies the user's configuration.");
        
        configCommand.AddCommand(new SetConfigCommand(_asyncCommandExecutor, configCommand.Name, _defaultSessionConfigIdOption));
        
        configCommand.AddCommand(new GenericFeatureConfigCommand(
            _asyncCommandExecutor, configCommand.Name, _featureArgument,
            "enable", "Enables a specified feature in the user's configuration."));
        
        configCommand.AddCommand(new GenericFeatureConfigCommand(
            _asyncCommandExecutor, configCommand.Name, _featureArgument,
            "disable", "Disables a specified feature in the user's configuration."));
        
        configCommand.AddCommand(new CreateSessionConfigCommand(_asyncCommandExecutor, configCommand.Name, _sessionConfigIdOption));
        configCommand.AddCommand(new ModifySessionConfigCommand(_asyncCommandExecutor, configCommand.Name, _sessionConfigIdOption));
        
        return configCommand;
    }
}
