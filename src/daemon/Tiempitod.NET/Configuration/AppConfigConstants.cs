namespace Tiempitod.NET.Configuration;

/// <summary>
/// Constants related to the application's configuration.
/// </summary>
public static class AppConfigConstants
{
    // Config dir names
    public const string RootConfigDirName = "tiempito";
    
    // Config file names
    public const string UserConfigFileName = "user.conf";
    
    // Parsing settings
    public const string UserSectionName = "User";
    public const string SessionSectionPrefix = "Session.";
    
    // Keyed Configuration Services
    public const string UserConfigFileProviderKey = "UserConfigFileProvider";
    public const string UserConfigParserServiceKey = "UserConfigParser";
}
