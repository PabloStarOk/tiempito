using Tiempitod.NET.Configuration.User;

namespace Tiempitod.NET.Commands.ConfigCommands;

/// <summary>
/// Represents the command to enable a user's feature configuration.
/// </summary>
public class EnableConfigCommand : ICommand
{
    private readonly IUserConfigProvider _userConfigProvider;
    private readonly IReadOnlyDictionary<string, string> _arguments;
    
    /// <summary>
    /// Instantiates a <see cref="EnableConfigCommand"/>
    /// </summary>
    /// <param name="userConfigProvider">Provider of user's configuration.</param>
    /// <param name="arguments">Parameters and their values to modify.</param>
    public EnableConfigCommand(
        IUserConfigProvider userConfigProvider,
        IReadOnlyDictionary<string, string> arguments)
    {
        _userConfigProvider = userConfigProvider;
        _arguments = arguments;
    }
    
    public Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!_arguments.TryGetValue("feature", out string? feature)
            || string.IsNullOrWhiteSpace(feature))
            return Task.FromResult(new OperationResult(Success: false, "Nothing to update."));

        if (!UserConfig.AllowedFeatures.Any(feat => feat.Name == feature || feat.Aliases.Contains(feature)))
            return Task.FromResult(new OperationResult(Success: false, $"Feature '{feature}' not recognized."));
        
        ConfigFeature configFeature = UserConfig.AllowedFeatures.First(f => f.Name == feature || f.Aliases.Contains(feature));
        
        if (_userConfigProvider.UserConfig.EnabledFeatures.Contains(configFeature.Name))
            return Task.FromResult(new OperationResult(Success: false, "Feature already enabled."));
        
        // TODO: Return operation result with custom message.
        UserConfig updatedUserConfig = _userConfigProvider.UserConfig;
        updatedUserConfig.AddFeature(configFeature);
        return Task.FromResult(_userConfigProvider.SaveUserConfig(updatedUserConfig));
    }
}