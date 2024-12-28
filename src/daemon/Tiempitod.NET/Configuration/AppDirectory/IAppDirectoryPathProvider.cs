namespace Tiempitod.NET.Configuration.AppDirectory;

/// <summary>
/// Defines a provider of application's config directory and user's config directory.
/// </summary>
public interface IAppDirectoryPathProvider
{
    /// <summary>
    /// Root path of the application's configuration directory shared by all users.
    /// </summary>
    public string AppConfigDirectoryPath { get; }
    
    /// <summary>
    /// Root path of the current user's configuration.
    /// </summary>
    public string UserConfigDirectoryPath { get; }
}
