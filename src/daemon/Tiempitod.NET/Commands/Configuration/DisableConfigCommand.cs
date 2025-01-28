using Tiempitod.NET.Configuration.User;

namespace Tiempitod.NET.Commands.Configuration;

/// <summary>
/// Represents the command to disable a user's feature configuration.
/// </summary>
public class DisableConfigCommand : ICommand
{
    private readonly IUserConfigProvider _userConfigProvider;
    private readonly IReadOnlyDictionary<string, string> _arguments;
    
    /// <summary>
    /// Instantiates a <see cref="DisableConfigCommand"/>
    /// </summary>
    /// <param name="userConfigProvider">Provider of user's configuration.</param>
    /// <param name="arguments">Parameters and their values to modify.</param>
    public DisableConfigCommand(
        IUserConfigProvider userConfigProvider,
        IReadOnlyDictionary<string, string> arguments)
    {
        _userConfigProvider = userConfigProvider;
        _arguments = arguments;
    }
    
    // TODO: Duplicated code
    public Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!_arguments.TryGetValue("feature", out string? feature)
            || string.IsNullOrWhiteSpace(feature))
            return Task.FromResult(new OperationResult(Success: false, "Nothing to update."));

        if (!UserConfig.AllowedFeatures.Any(feat => feat.Name == feature || feat.Aliases.Contains(feature)))
            return Task.FromResult(new OperationResult(Success: false, $"Feature '{feature}' not recognized."));
        
        ConfigFeature configFeature = UserConfig.AllowedFeatures.First(f => f.Name == feature || f.Aliases.Contains(feature));
        
        if (!_userConfigProvider.UserConfig.EnabledFeatures.Contains(configFeature.Name))
            return Task.FromResult(new OperationResult(Success: false, "Feature already disabled."));
        
        UserConfig updatedUserConfig = _userConfigProvider.UserConfig;
        updatedUserConfig.RemoveFeature(configFeature);

        OperationResult savingOperationResult = _userConfigProvider.SaveUserConfig(updatedUserConfig);
        OperationResult result = savingOperationResult with
        {
            Message = savingOperationResult.Success ? "Feature modified." : "Feature cannot"
        };
        
        return Task.FromResult(result);
    }
}
