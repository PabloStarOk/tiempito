using Tiempitod.NET.Configuration.User;

namespace Tiempitod.NET.Commands.Configuration;

/// <summary>
/// Represents the command to disable a user's feature configuration.
/// </summary>
/// <param name="userConfigProvider">Provider of user's configuration.</param>
/// <param name="arguments">Parameters and their values to modify.</param>
public readonly struct DisableConfigCommand(
    IUserConfigProvider userConfigProvider,
    IReadOnlyDictionary<string, string> arguments)
    : ICommand
{
    // TODO: Duplicated code
    public Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!arguments.TryGetValue("feature", out string? feature)
            || string.IsNullOrWhiteSpace(feature))
            return Task.FromResult(new OperationResult(Success: false, "Nothing to update."));

        if (!UserConfig.AllowedFeatures.Any(feat => feat.Name == feature || feat.Aliases.Contains(feature)))
            return Task.FromResult(new OperationResult(Success: false, $"Feature '{feature}' not recognized."));
        
        ConfigFeature configFeature = UserConfig.AllowedFeatures.First(f => f.Name == feature || f.Aliases.Contains(feature));
        
        if (!userConfigProvider.UserConfig.EnabledFeatures.Contains(configFeature.Name))
            return Task.FromResult(new OperationResult(Success: false, "Feature already disabled."));
        
        UserConfig updatedUserConfig = userConfigProvider.UserConfig;
        updatedUserConfig.RemoveFeature(configFeature);

        OperationResult savingOperationResult = userConfigProvider.SaveUserConfig(updatedUserConfig);
        OperationResult result = savingOperationResult with
        {
            Message = savingOperationResult.Success ? "Feature modified." : "Feature cannot"
        };
        
        return Task.FromResult(result);
    }
}
