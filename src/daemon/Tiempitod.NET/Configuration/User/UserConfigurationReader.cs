using Salaros.Configuration;

namespace Tiempitod.NET.Configuration.User;

/// <summary>
/// Reads the user's configuration.
/// </summary>
public class UserConfigurationReader : IUserConfigurationReader
{
    private readonly ConfigParser _configParser;
    
    public UserConfigurationReader(
        [FromKeyedServices(AppConfigConstants.UserConfigParserServiceKey)] ConfigParser configParser)
    {
        _configParser = configParser;
    }
    
    // TODO: Make method asynchronous.
    public UserConfiguration Read()
    {
        var userConfig = new UserConfiguration();

        if (_configParser[AppConfigConstants.UserSectionName] == null)
            return userConfig;
            
        ConfigSection configSection = _configParser[AppConfigConstants.UserSectionName];

        foreach (IConfigKeyValue keyValue in configSection.Keys)
        {
            string keywordString = keyValue.Name.ToLower();

            if (Enum.TryParse(keywordString, out UserConfigurationKeyword keyword))
                continue;
            
            if (keyword is UserConfigurationKeyword.DefaultSession)
                userConfig = new UserConfiguration(keyValue.Content);
        }
        
        return userConfig;
    }
}
