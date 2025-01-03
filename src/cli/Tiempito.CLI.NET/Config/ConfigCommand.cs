using System.CommandLine;
using Tiempito.CLI.NET.Client;

namespace Tiempito.CLI.NET.Config;

/// <summary>
/// Provides command and group of commands related to user's configuration.
/// </summary>
public class ConfigCommand
{
    private readonly IClient _client;
    private readonly Option<string> _defaultSessionIdOption;

    /// <summary>
    /// Instantiates a new <see cref="ConfigCommand"/>.
    /// </summary>
    /// <param name="client">Client to use in each subcommand to send requests.</param>
    public ConfigCommand(IClient client)
    {
        _client = client;
        _defaultSessionIdOption = new Option<string>("--default-session", "Sets the default session configuration to use.")
        {
            Arity = ArgumentArity.ExactlyOne
        };
        _defaultSessionIdOption.AddAlias("-d");
    }

    /// <summary>
    /// Creates a new <see cref="Command"/> named config.
    /// </summary>
    /// <returns>A configured <see cref="Command"/>.</returns>
    public Command GetCommand()
    {
        var configCommand = new Command("config", "Modifies the user's configuration.");
        
        configCommand.AddCommand(new SetConfigCommand(_client, _defaultSessionIdOption));

        return configCommand;
    }
}
