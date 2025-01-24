namespace Tiempitod.NET.Configuration.AppFilesystem;

/// <summary>
/// Provides access to application's config directory and user's config directory paths and creates user's config directory if it doesn't exist.
/// </summary>
public class AppFilesystemPathProvider : IAppFilesystemPathProvider
{
    private readonly static string CommonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
    private readonly static string UserAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    
    public string AppConfigDirectoryPath { get; }
    public string UserConfigDirectoryPath { get; }
    public string DaemonConfigFilePath { get; }
    public string ApplicationIconPath { get; }

    /// <summary>
    /// Instantiates a new <see cref="AppFilesystemPathProvider"/> and creates user's config directory if it doesn't exist.
    /// </summary>
    /// <param name="logger">Logger to register events.</param>
    /// <exception cref="ArgumentException">If the application's shared configuration directory doesn't exist.</exception>
    public AppFilesystemPathProvider(ILogger<AppFilesystemPathProvider> logger)
    {
        AppConfigDirectoryPath = Path.Combine(CommonAppData, AppConfigConstants.RootConfigDirName);
        UserConfigDirectoryPath = Path.Combine(UserAppData, AppConfigConstants.RootConfigDirName);
        DaemonConfigFilePath = Path.Combine(CommonAppData, AppConfigConstants.RootConfigDirName, AppConfigConstants.DaemonConfigFileName);
        ApplicationIconPath = Path.Combine(CommonAppData, AppConfigConstants.RootConfigDirName, AppConfigConstants.IconFileName);
        
        if (!Directory.Exists(AppConfigDirectoryPath))
        {
            logger.LogCritical("Application's configuration directory doesn't exist at {Path}", AppConfigDirectoryPath);
            throw new ArgumentException("Application's configuration directory doesn't exist");
        }

        Directory.CreateDirectory(UserConfigDirectoryPath);
    }
}
