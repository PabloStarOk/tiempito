using Tiempito.Daemon.Application.Config;

namespace Tiempito.Daemon.Application.Shared;

/// <summary>
/// Provides common application and user-specific paths used throughout the daemon.
/// </summary>
internal static class Paths
{
    /// <summary>
    /// Gets the directory path where the daemon's configuration files are stored,
    /// located under the system-wide common application data directory.
    /// </summary>
    public static readonly string DaemonConfigDirectoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        AppConfigConstants.RootConfigDirName);

    /// <summary>
    /// Gets the full file path to the daemon's main configuration file,
    /// located within the system-wide configuration directory.
    /// </summary>
    public static readonly string DaemonConfigFilePath = Path.Combine(
        DaemonConfigDirectoryPath,
        AppConfigConstants.DaemonConfigFileName);

    /// <summary>
    /// Gets the full file path to the user-specific configuration file,
    /// located within the user's application data directory.
    /// </summary>
    public static readonly string UserConfigFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        AppConfigConstants.RootConfigDirName,
        AppConfigConstants.UserConfigFileName);

    /// <summary>
    /// Gets the full file path to the application's icon,
    /// stored in the daemon's configuration directory.
    /// </summary>
    public static readonly string ApplicationIconPath = Path.Combine(
        DaemonConfigDirectoryPath,
        AppConfigConstants.IconFileName);
}