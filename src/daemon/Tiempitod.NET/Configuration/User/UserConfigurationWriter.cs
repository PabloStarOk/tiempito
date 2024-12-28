using Salaros.Configuration;

namespace Tiempitod.NET.Configuration.User;

/// <summary>
/// Writes in the user's configuration.
/// </summary>
public class UserConfigurationWriter : IUserConfigurationWriter
{
    private readonly ConfigParser _configParser;
    
    /// <summary>
    /// Instantiates a new <see cref="UserConfigurationWriter"/>
    /// </summary>
    /// <param name="configParser">Parser of the user's configuration file.</param>
    public UserConfigurationWriter(
        [FromKeyedServices(AppConfigConstants.UserConfigParserServiceKey)] ConfigParser configParser)
    {
        _configParser = configParser;
    }
    
    // TODO: Make method asynchronous.
    public bool Write(UserConfiguration userConfig)
    {
        return _configParser.SetValue(
                AppConfigConstants.UserSectionName,
                UserConfigurationKeyword.DefaultSession.ToString(),
                userConfig.DefaultSessionId)
            && _configParser.Save();
    }
}
