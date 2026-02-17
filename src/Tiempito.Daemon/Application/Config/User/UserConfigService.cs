using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models.Enums;

namespace Tiempito.Daemon.Application.Config.User;

/// <summary>
/// Service for managing the user's configuration.
/// </summary>
public class UserConfigService : IUserConfigService, IHostedService
{
    private readonly ILogger<UserConfigService> _logger;
    private readonly IOptionsMonitor<UserConfig> _userConfigMonitor;
    private readonly IUserConfigWriter _userConfigWriter;
    private readonly IFileProvider _userDirectoryFileProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConfigService"/> class.
    /// </summary>
    /// <param name="logger">Logger to register events.</param>
    /// <param name="userConfigMonitor">Monitor for user's configuration.</param>
    /// <param name="userConfigWriter">Writer for user's configuration.</param>
    /// <param name="userDirectoryFileProvider">Provider of files for the user's configuration directory.</param>
    public UserConfigService(
        ILogger<UserConfigService> logger,
        IOptionsMonitor<UserConfig> userConfigMonitor,
        IUserConfigWriter userConfigWriter,
        IFileProvider userDirectoryFileProvider)
    {
        _logger = logger;
        _userConfigMonitor = userConfigMonitor;
        _userDirectoryFileProvider = userDirectoryFileProvider;
        _userConfigWriter = userConfigWriter;
    }

    /// <inheritdoc/>
    public Task<OperationResult> ChangeDefaultSessionConfigAsync(string? id)
    {
        var currentDefaultId = _userConfigMonitor.CurrentValue.DefaultConfigId;
        if (currentDefaultId?.Equals(id, StringComparison.OrdinalIgnoreCase) == true)
        {
            var result = new OperationResult(Success: false, Message: "Provided ID is already set as default.");
            return Task.FromResult(result);
        }

        OperationResult operationResult = SaveAndReturnResult(
            _userConfigMonitor.CurrentValue with { DefaultConfigId = id },
            successMessage: "Default session config ID was changed.",
            errorMessage: "Default session config ID couldn't be changed in the configuration file.");

        if (operationResult.Success)
        {
            _logger.LogDebug("Session configuration with ID '{Id}' has been set as default.", id);
        }
        else
        {
            _logger.LogError("Session configuration with ID '{Id}' could not be set as default.", id);
        }

        return Task.FromResult(operationResult);
    }

    /// <inheritdoc/>
    public Task<OperationResult> EnableFeatureAsync(UserFeature feature)
    {
        // 1. Verify if the is already enabled.
        if (_userConfigMonitor.CurrentValue.EnabledFeatures.Contains(feature))
        {
            return Task.FromResult(new OperationResult(
                Success: false,
                Message: $"Feature {feature.ToString().ToLower()} is already enabled."));
        }

        // 2. Enable feature.
        OperationResult operationResult = SaveAndReturnResult(
            _userConfigMonitor.CurrentValue.WithEnabledFeature(feature),
            successMessage: "Feature enabled",
            errorMessage: "Feature couldn't be enabled in the configuration file.");

        if (operationResult.Success)
        {
            _logger.LogDebug("Feature '{Feature}' has been enabled.", feature);
        }

        return Task.FromResult(operationResult);
    }

    /// <inheritdoc/>
    public Task<OperationResult> DisableFeatureAsync(UserFeature feature)
    {
        // 1. Verify if the is already disabled.
        if (!_userConfigMonitor.CurrentValue.EnabledFeatures.Contains(feature))
        {
            return Task.FromResult(new OperationResult(
                Success: false,
                Message: $"Feature {feature.ToString().ToLower()} is already disabled."));
        }

        // 2. Disable feature.
        OperationResult operationResult = SaveAndReturnResult(
            _userConfigMonitor.CurrentValue.WithDisabledFeature(feature),
            successMessage: "Feature disabled",
            errorMessage: "Feature couldn't be disabled in the configuration file.");

        if (operationResult.Success)
        {
            _logger.LogDebug("Feature '{Feature}' has been disabled.", feature);
        }

        return Task.FromResult(operationResult);
    }

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await CreateUserConfigAsync();
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates the user configuration if not exists.
    /// </summary>
    private async Task CreateUserConfigAsync()
    {
        IFileInfo fileInfo = _userDirectoryFileProvider.GetFileInfo(AppConfigConstants.UserConfigFileName);

        if (fileInfo.Exists || string.IsNullOrWhiteSpace(fileInfo.PhysicalPath))
        {
            return;
        }

        await File.Create(fileInfo.PhysicalPath).DisposeAsync();
        _logger.LogInformation("User's configuration file was created at {Path}", fileInfo.PhysicalPath);
    }

    /// <summary>
    /// Saves the user configuration in the file.
    /// </summary>
    /// <param name="userConfig">The user configuration to save.</param>
    /// <param name="successMessage">Message to use when the configuration is saved successfully.</param>
    /// <param name="errorMessage">Message to use when an error occurs.</param>
    /// <returns>An <see cref="OperationResult"/>.</returns>
    private OperationResult SaveAndReturnResult(UserConfig userConfig, string successMessage, string errorMessage)
    {
        bool wasSaved = _userConfigWriter.Write(userConfig);
        return new OperationResult(wasSaved, wasSaved ? successMessage : errorMessage);
    }
}