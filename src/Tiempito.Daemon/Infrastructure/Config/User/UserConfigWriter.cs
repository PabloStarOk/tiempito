using System.Text;

using Salaros.Configuration;

using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Config.User;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Config.Enums;

namespace Tiempito.Daemon.Infrastructure.Config.User;

/// <summary>
/// Writes in the user's configuration.
/// </summary>
public class UserConfigWriter : IUserConfigWriter
{
    private readonly ConfigParser _configParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConfigWriter"/> class.
    /// </summary>
    /// <param name="configParser">Parser of the user's configuration file.</param>
    public UserConfigWriter(
        ConfigParser configParser)
    {
        _configParser = configParser;
    }

    // TODO: Make method asynchronous.

    /// <inheritdoc/>
    public bool Write(UserConfig userConfig)
    {
        bool wasSessionIdSet = _configParser.SetValue(
            AppConfigConstants.UserSectionName,
            nameof(UserConfigKeyword.DefaultSession),
            userConfig.DefaultSessionId);

        var enabledFeatures = new StringBuilder()
            .AppendJoin(',', userConfig.EnabledFeatures)
            .ToString();

        bool wasEnabledFeaturesSet = _configParser.SetValue(
            AppConfigConstants.UserSectionName,
            nameof(UserConfigKeyword.EnabledFeatures),
            enabledFeatures);

        return wasSessionIdSet && wasEnabledFeaturesSet && _configParser.Save();
    }
}
