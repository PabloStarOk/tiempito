namespace Tiempito.Daemon.Application.Config;

/// <summary>
/// Constants related to the application's configuration.
/// </summary>
public static class AppConfigConstants
{
    /// <summary>
    /// Name of the root configuration directory.
    /// </summary>
    public const string RootConfigDirName = "tiempito";

    /// <summary>
    /// Name of the icon file used in the application.
    /// </summary>
    public const string IconFileName = "icon.png";

    /// <summary>
    /// Name of the daemon configuration file.
    /// </summary>
    public const string DaemonConfigFileName = "tiempitod.conf";

    /// <summary>
    /// Name of the user configuration file.
    /// </summary>
    public const string UserConfigFileName = "user.conf";

    /// <summary>
    /// Section name for user settings in configuration files.
    /// </summary>
    public const string UserSectionName = "User";

    /// <summary>
    /// Prefix for session section names in configuration files.
    /// </summary>
    public const string SessionSectionPrefix = "Session.";
}
