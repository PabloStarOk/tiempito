using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Config.User;
using Tiempito.Daemon.Domain.Config;
using Tiempito.IPC.Models.Enums;

namespace Tiempito.Daemon.Infrastructure.Config.User;

/// <summary>
/// Reads the user's configuration.
/// </summary>
public class UserConfigReader : IUserConfigReader
{
    private readonly ILogger<UserConfigReader> _logger;
    private readonly IConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConfigReader"/> class.
    /// </summary>
    /// <param name="logger">Logger to register events.</param>
    /// <param name="config">Configuration provider.</param>
    public UserConfigReader(ILogger<UserConfigReader> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    /// <inheritdoc/>
    public UserConfig Read()
    {
        var userConfig = new UserConfig();
        var rawUserConfig = _config.GetSection(AppConfigConstants.UserSectionName).Get<RawUserConfig>();
        if (rawUserConfig is null)
        {
            return userConfig;
        }

        var featureStrings = rawUserConfig.EnabledFeatures.Split(
            AppConfigConstants.IniArraySeparator,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        List<UserFeature> enabledFeatures = [];
        foreach (string featString in featureStrings)
        {
            if (!Enum.TryParse(featString, out UserFeature feature))
            {
                continue;
            }

            enabledFeatures.Add(feature);
        }

        userConfig.SetDefaultSessionConfigId(rawUserConfig.DefaultConfigId);
        foreach (UserFeature feature in enabledFeatures)
        {
            userConfig.EnableFeature(feature);
        }

        _logger.LogInformation("User's configuration read");
        _logger.LogTrace(
            "User's configuration read: Default config ID: {DefaultId}, Enabled features: {EnabledFeatures}",
            userConfig.DefaultConfigId,
            userConfig.EnabledFeatures);
        return userConfig;
    }

    internal sealed record RawUserConfig(
        string? DefaultConfigId,
        string EnabledFeatures);
}
