using Salaros.Configuration;

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
        return _configParser.SetValue(
                AppConfigConstants.UserSectionName,
                UserConfigKeyword.DefaultSession.ToString(),
                userConfig.DefaultSessionId)
            && _configParser.Save();
    }
}
