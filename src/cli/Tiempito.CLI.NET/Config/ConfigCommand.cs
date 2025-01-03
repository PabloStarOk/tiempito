using System.CommandLine;
using Tiempito.CLI.NET.Client;

namespace Tiempito.CLI.NET.Config;

/// <summary>
/// Provides command and group of commands related to user's configuration.
/// </summary>
public class ConfigCommand
{
    private readonly IAsyncCommandExecutor _asyncCommandExecutor;
    private readonly Option<string> _defaultSessionIdOption;
    private readonly Argument<string> _featureArgument;

    /// <summary>
    /// Instantiates a new <see cref="ConfigCommand"/>.
    /// </summary>
    /// <param name="asyncCommandExecutor">An asynchronous executor of commands.</param>
    public ConfigCommand(IAsyncCommandExecutor asyncCommandExecutor)
    {
        _asyncCommandExecutor = asyncCommandExecutor;
        _defaultSessionIdOption = new Option<string>("--default-session", "Sets the default session configuration to use.")
        {
            Arity = ArgumentArity.ExactlyOne
        };
        _defaultSessionIdOption.AddAlias("-d");
        
        _featureArgument = new Argument<string>("feature", "Feature to enable or disable.")
        {
            Arity = ArgumentArity.ExactlyOne
        };
    }

    /// <summary>
    /// Creates a new <see cref="Command"/> named config.
    /// </summary>
    /// <returns>A configured <see cref="Command"/>.</returns>
    public Command GetCommand()
    {
        var configCommand = new Command("config", "Modifies the user's configuration.");
        
        configCommand.AddCommand(new SetConfigCommand(_asyncCommandExecutor, configCommand.Name, _defaultSessionIdOption));
        configCommand.AddCommand(new EnableConfigCommand(_asyncCommandExecutor, configCommand.Name, _featureArgument));
        configCommand.AddCommand(new DisableConfigCommand(_asyncCommandExecutor, configCommand.Name, _featureArgument));
        
        return configCommand;
    }
}
