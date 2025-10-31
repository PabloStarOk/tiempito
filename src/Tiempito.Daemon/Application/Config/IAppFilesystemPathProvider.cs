namespace Tiempito.Daemon.Application.Config;

/// <summary>
/// Defines a provider of application's config directory and user's config directory.
/// </summary>
public interface IAppFilesystemPathProvider
{
    /// <summary>
    /// Gets the root path of the application's configuration directory shared by all users.
    /// </summary>
    public string AppConfigDirectoryPath { get; }

    /// <summary>
    /// Gets the root path of the current user's configuration.
    /// </summary>
    public string UserConfigDirectoryPath { get; }

    /// <summary>
    /// Gets the path of daemon's configuration file.
    /// </summary>
    public string DaemonConfigFilePath { get; }

    /// <summary>
    /// Gets the path of the application's icon.
    /// </summary>
    public string ApplicationIconPath { get; }
}
