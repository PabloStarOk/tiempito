using Microsoft.Extensions.FileProviders;

using Tiempitod.NET.Common;

namespace Tiempitod.NET.Configuration.User;

/// <summary>
/// Service for manage user's configuration.
/// </summary>
public class UserConfigService : Service, IUserConfigService
{
    private readonly IUserConfigReader _userConfigReader;
    private readonly IUserConfigWriter _userConfigWriter;
    private readonly IFileProvider _userDirectoryFileProvider;

    public UserConfig UserConfig { get; private set; }
    public event EventHandler? OnConfigChanged;

    /// <summary>
    /// Instantiates a new <see cref="UserConfigService"/>
    /// </summary>
    /// <param name="logger">Logger to register events.</param>
    /// <param name="userConfigReader">Reader for user's configuration.</param>
    /// <param name="userConfigWriter">Writer for user's configuration.</param>
    /// <param name="userDirectoryFileProvider">Provider of files for the user's configuration directory.</param>
    public UserConfigService(
        ILogger<UserConfigService> logger,
        IUserConfigReader userConfigReader,
        IUserConfigWriter userConfigWriter,
        [FromKeyedServices(AppConfigConstants.UserConfigFileProviderKey)] IFileProvider userDirectoryFileProvider)
        : base(logger)
    {
        _userDirectoryFileProvider = userDirectoryFileProvider;
        _userConfigReader = userConfigReader;
        _userConfigWriter = userConfigWriter;
    }

    /// <inheritdoc/>
    public Task<OperationResult> ChangeDefaultSessionConfigAsync(string id)
    {
        UserConfig.SetDefaultSessionConfigId(id);
        
        OperationResult operationResult = SaveAndReturnResult(
            successMessage: "Default session config ID was changed.",
            errorMessage: "Default session config ID couldn't be changed in the configuration file.");
        
        return Task.FromResult(operationResult);
    }

    /// <inheritdoc/>
    public Task<OperationResult> EnableFeatureAsync(string feature)
    {
        if (!FeatureExists(feature, out OperationResult result))
            return Task.FromResult(result);
        
        UserConfigFeature configFeature = UserConfig.AllowedFeatures.First(
            f => f.Name == feature || f.Aliases.Contains(feature));
        
        // 1. Verify if the is already enabled.
        if (UserConfig.EnabledFeatures.Contains(configFeature.Name))
        {
            return Task.FromResult(new OperationResult(
                Success: false,
                Message: $"Feature {configFeature.Name} is already enabled."));   
        }
        
        // 2. Enable feature.
        UserConfig.AddFeature(configFeature);
        
        OperationResult operationResult = SaveAndReturnResult(
            successMessage: "Feature enabled",
            errorMessage: "Feature couldn't be enabled in the configuration file.");
        
        return Task.FromResult(operationResult);
    }
    
    /// <inheritdoc/>
    public Task<OperationResult> DisableFeatureAsync(string feature)
    {
        if (!FeatureExists(feature, out OperationResult result))
            return Task.FromResult(result);
        
        UserConfigFeature configFeature = UserConfig.AllowedFeatures.First(
            f => f.Name == feature || f.Aliases.Contains(feature));
        
        // 1. Verify if the is already disabled.
        if (!UserConfig.EnabledFeatures.Contains(configFeature.Name))
        {
            return Task.FromResult(new OperationResult(
                Success: false,
                Message: $"Feature {configFeature.Name} is already disabled."));   
        }

        // 2. Disable feature.
        UserConfig.RemoveFeature(configFeature);

        OperationResult operationResult = SaveAndReturnResult(
            successMessage: "Feature disabled",
            errorMessage: "Feature couldn't be disabled in the configuration file.");
        
        return Task.FromResult(operationResult);
    }
    
    /// <inheritdoc/>
    protected override async Task<bool> OnStartServiceAsync()
    {
        await CreateUserConfigAsync();
        UserConfig = _userConfigReader.Read();
        return true;
    }

    /// <inheritdoc/>
    protected override Task<bool> OnStopServiceAsync()
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// Creates the user configuration if not exists.
    /// </summary>
    private async Task CreateUserConfigAsync()
    {
        IFileInfo fileInfo = _userDirectoryFileProvider.GetFileInfo(AppConfigConstants.UserConfigFileName);
        
        if (fileInfo.Exists || string.IsNullOrWhiteSpace(fileInfo.PhysicalPath))
            return;
        
        await File.Create(fileInfo.PhysicalPath).DisposeAsync();
        _logger.LogInformation("User config filed was created at {Path}", fileInfo.PhysicalPath);
    }

    /// <summary>
    /// Checks if a string matches a feature name
    /// or alias.
    /// </summary>
    /// <param name="feature">Feature to compare.</param>
    /// <param name="operationResult">A <see cref="OperationResult"/>.</param>
    /// <returns>True if the feature exists, false otherwise.</returns>
    private static bool FeatureExists(string feature, out OperationResult operationResult)
    {
        operationResult = new OperationResult(true, string.Empty);

        if (UserConfig.AllowedFeatures.Any
            (
                f => f.Name == feature
                    || f.Aliases.Contains(feature)
            ))
        {
            return true;
        }

        operationResult = new OperationResult(
            Success: false,
            Message: $"Feature {feature} not recognized.");
        return false;

    }
    
    /// <summary>
    /// Saves the user configuration in the file.
    /// </summary>
    /// <param name="successMessage">Message to use when the configuration is saved successfully.</param>
    /// <param name="errorMessage">Message to use when an error occurs.</param>
    /// <returns>An <see cref="OperationResult"/>.</returns>
    private OperationResult SaveAndReturnResult(string successMessage, string errorMessage)
    {
        bool wasSaved = _userConfigWriter.Write(UserConfig);
        
        if (wasSaved)
            OnConfigChanged?.Invoke(this, EventArgs.Empty);
        
        return new OperationResult(wasSaved, wasSaved ? successMessage : errorMessage);
    }
}