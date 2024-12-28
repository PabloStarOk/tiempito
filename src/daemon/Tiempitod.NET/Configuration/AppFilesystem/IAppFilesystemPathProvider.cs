namespace Tiempitod.NET.Configuration.AppFilesystem;

/// <summary>
/// Defines a provider of application's config directory and user's config directory.
/// </summary>
public interface IAppFilesystemPathProvider
{
    /// <summary>
    /// Root path of the application's configuration directory shared by all users.
    /// </summary>
    public string AppConfigDirectoryPath { get; }
    
    /// <summary>
    /// Root path of the current user's configuration.
    /// </summary>
    public string UserConfigDirectoryPath { get; }
    
    /// <summary>
    /// Path of daemon's configuration file.
    /// </summary>
    public string DaemonConfigFilePath { get; }
}
