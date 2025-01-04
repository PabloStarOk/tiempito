using Tiempitod.NET.Configuration.User;

namespace Tiempitod.NET.Commands.ConfigCommands;

/// <summary>
/// Represents the subcommand to modify parameters of the user's configuration.
/// </summary>
public class SetConfigCommand : ICommand
{
    private readonly IUserConfigProvider _userConfigProvider;
    private readonly IReadOnlyDictionary<string, string> _arguments;
    
    /// <summary>
    /// Instantiates a <see cref="SetConfigCommand"/>
    /// </summary>
    /// <param name="userConfigProvider">Provider of user's configuration.</param>
    /// <param name="arguments">Parameters and their values to modify.</param>
    public SetConfigCommand(IUserConfigProvider userConfigProvider, IReadOnlyDictionary<string, string> arguments)
    {
        _userConfigProvider = userConfigProvider;
        _arguments = arguments;
    }
    
    public Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!_arguments.TryGetValue("default-session-id", out string? defaultSessionId)
            || string.IsNullOrWhiteSpace(defaultSessionId))
            return Task.FromResult(new OperationResult(Success: false, "Nothing to update."));
        
        var newUserConfig = new UserConfig(defaultSessionId);
        return Task.FromResult(_userConfigProvider.SaveUserConfig(newUserConfig));
    }
}
