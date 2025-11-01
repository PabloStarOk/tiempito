using Salaros.Configuration;

using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Config.User;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Config.Enums;
using Tiempito.IPC.Models.Enums;

namespace Tiempito.Daemon.Infrastructure.Config.User;

/// <summary>
/// Reads the user's configuration.
/// </summary>
public class UserConfigReader : IUserConfigReader
{
    private readonly ConfigParser _configParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConfigReader"/> class.
    /// </summary>
    /// <param name="configParser">Parser of the user's configuration file.</param>
    public UserConfigReader(
        ConfigParser configParser)
    {
        _configParser = configParser;
    }

    // TODO: Make method asynchronous.

    /// <inheritdoc/>
    public UserConfig Read()
    {
        var userConfig = default(UserConfig);

        if (_configParser[AppConfigConstants.UserSectionName] == null)
        {
            return userConfig;
        }

        ConfigSection configSection = _configParser[AppConfigConstants.UserSectionName];

        foreach (IConfigKeyValue keyValue in configSection.Keys)
        {
            string keywordString = keyValue.Name;
            if (!Enum.TryParse(keywordString, ignoreCase: true, out UserConfigKeyword keyword))
            {
                continue;
            }

            switch (keyword)
            {
                case UserConfigKeyword.DefaultSession:
                    userConfig = new UserConfig(keyValue.Content);
                    break;

                case UserConfigKeyword.EnabledFeatures:
                    foreach (string enabledFeatString in keyValue.Content.Split(','))
                    {
                        if (string.IsNullOrWhiteSpace(enabledFeatString) ||
                            !Enum.TryParse(enabledFeatString.Trim(), ignoreCase: true, out UserFeature feature))
                        {
                            continue;
                        }

                        userConfig.AddFeature(feature);
                    }

                    break;

                default:
                    continue;
            }
        }

        return userConfig;
    }
}
