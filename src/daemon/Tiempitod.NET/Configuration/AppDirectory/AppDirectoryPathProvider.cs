namespace Tiempitod.NET.Configuration.AppDirectory;

/// <summary>
/// Provides access to application's config directory and user's config directory paths and creates user's config directory if it doesn't exist.
/// </summary>
public class AppDirectoryPathProvider : IAppDirectoryPathProvider
{
    public string AppConfigDirectoryPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "tiempito");
    public string UserConfigDirectoryPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "tiempito");
    
    /// <summary>
    /// Instantiates a new <see cref="AppDirectoryPathProvider"/> and creates user's config directory if it doesn't exist.
    /// </summary>
    /// <param name="logger">Logger to register events.</param>
    /// <exception cref="ArgumentException">If the application's shared configuration directory doesn't exist.</exception>
    public AppDirectoryPathProvider(ILogger<AppDirectoryPathProvider> logger)
    {
        if (!Directory.Exists(AppConfigDirectoryPath))
        {
            logger.LogCritical("Application's configuration directory doesn't exist at {Path}", AppConfigDirectoryPath);
            throw new ArgumentException("Application's configuration directory doesn't exist");
        }

        Directory.CreateDirectory(UserConfigDirectoryPath);
    }
}
