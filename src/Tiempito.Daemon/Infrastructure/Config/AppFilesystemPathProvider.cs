using Tiempito.Daemon.Application.Config;

namespace Tiempito.Daemon.Infrastructure.Config;

/// <summary>
/// Provides access to application's config directory and user's config directory paths and creates user's config directory if it doesn't exist.
/// </summary>
public class AppFilesystemPathProvider : IAppFilesystemPathProvider
{
    /// <inheritdoc/>
    public string AppConfigDirectoryPath { get; }

    /// <inheritdoc/>
    public string UserConfigDirectoryPath { get; }

    /// <inheritdoc/>
    public string DaemonConfigFilePath { get; }

    /// <inheritdoc/>
    public string ApplicationIconPath { get; }

    private static readonly string CommonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
    private static readonly string UserAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    /// <summary>
    /// Initializes a new instance of the <see cref="AppFilesystemPathProvider"/> class.
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
