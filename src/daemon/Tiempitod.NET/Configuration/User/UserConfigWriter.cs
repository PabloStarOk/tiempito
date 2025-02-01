using Salaros.Configuration;
using System.Text;

using Tiempitod.NET.Configuration.User.Enums;
using Tiempitod.NET.Configuration.User.Interfaces;
using Tiempitod.NET.Configuration.User.Objects;

namespace Tiempitod.NET.Configuration.User;

/// <summary>
/// Writes in the user's configuration.
/// </summary>
public class UserConfigWriter : IUserConfigWriter
{
    private readonly ConfigParser _configParser;
    
    /// <summary>
    /// Instantiates a new <see cref="UserConfigWriter"/>
    /// </summary>
    /// <param name="configParser">Parser of the user's configuration file.</param>
    public UserConfigWriter(
        [FromKeyedServices(AppConfigConstants.UserConfigParserServiceKey)] ConfigParser configParser)
    {
        _configParser = configParser;
    }
    
    // TODO: Make method asynchronous.
    public bool Write(UserConfig userConfig)
    {
        bool wasSessionIdSet = _configParser.SetValue
        (
            AppConfigConstants.UserSectionName,
            UserConfigKeyword.DefaultSession.ToString(),
            userConfig.DefaultSessionId
        );
        
        var enabledFeatures = new StringBuilder()
            .AppendJoin(',', userConfig.EnabledFeatures)
            .ToString();
        
        bool wasEnabledFeaturesSet = _configParser.SetValue
        (
            AppConfigConstants.UserSectionName,
            UserConfigKeyword.EnabledFeatures.ToString(),
            enabledFeatures
        );
        
        return wasSessionIdSet && wasEnabledFeaturesSet && _configParser.Save();
    }
}
