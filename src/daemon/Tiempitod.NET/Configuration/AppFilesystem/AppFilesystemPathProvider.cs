namespace Tiempitod.NET.Configuration.AppFilesystem;

/// <summary>
/// Provides access to application's config directory and user's config directory paths and creates user's config directory if it doesn't exist.
/// </summary>
public class AppFilesystemPathProvider : IAppFilesystemPathProvider
{
    public string AppConfigDirectoryPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), AppConfigConstants.RootConfigDirName);
    public string UserConfigDirectoryPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppConfigConstants.RootConfigDirName);
    public string DaemonConfigFilePath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), AppConfigConstants.RootConfigDirName, AppConfigConstants.DaemonConfigFileName);

    /// <summary>
    /// Instantiates a new <see cref="AppFilesystemPathProvider"/> and creates user's config directory if it doesn't exist.
    /// </summary>
    /// <param name="logger">Logger to register events.</param>
    /// <exception cref="ArgumentException">If the application's shared configuration directory doesn't exist.</exception>
    public AppFilesystemPathProvider(ILogger<AppFilesystemPathProvider> logger)
    {
        if (!Directory.Exists(AppConfigDirectoryPath))
        {
            logger.LogCritical("Application's configuration directory doesn't exist at {Path}", AppConfigDirectoryPath);
            throw new ArgumentException("Application's configuration directory doesn't exist");
        }

        Directory.CreateDirectory(UserConfigDirectoryPath);
    }
}
