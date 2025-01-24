using Salaros.Configuration;

namespace Tiempitod.NET.Configuration.User;

/// <summary>
/// Reads the user's configuration.
/// </summary>
public class UserConfigReader : IUserConfigReader
{
    private readonly ConfigParser _configParser;
    
    /// <summary>
    /// Instantiates a new <see cref="UserConfigReader"/> 
    /// </summary>
    /// <param name="configParser">Parser of the user's configuration file.</param>
    public UserConfigReader(
        [FromKeyedServices(AppConfigConstants.UserConfigParserServiceKey)] ConfigParser configParser)
    {
        _configParser = configParser;
    }
    
    // TODO: Make method asynchronous.
    public UserConfig Read()
    {
        var userConfig = new UserConfig();

        if (_configParser[AppConfigConstants.UserSectionName] == null)
            return userConfig;
            
        ConfigSection configSection = _configParser[AppConfigConstants.UserSectionName];

        foreach (IConfigKeyValue keyValue in configSection.Keys)
        {
            string keywordString = keyValue.Name;

            if (!Enum.TryParse(keywordString, ignoreCase: true, out UserConfigKeyword keyword))
                continue;

            switch (keyword)
            {
                case UserConfigKeyword.DefaultSession:
                    userConfig = new UserConfig(keyValue.Content);
                    break;
                    
                case UserConfigKeyword.EnabledFeatures:
                    foreach (string enabledFeat in keyValue.Content.Split(','))
                    {
                        if (string.IsNullOrWhiteSpace(enabledFeat))
                            continue;
                        
                        userConfig.AddFeature(enabledFeat.Trim());
                    }
                    break;
                
                default:
                    continue;
            }
        }
        
        return userConfig;
    }
}
