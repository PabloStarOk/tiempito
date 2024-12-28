using Microsoft.Extensions.FileProviders;
using Tiempitod.NET.Configuration.AppDirectory;
using Tiempitod.NET.Configuration.Session;

namespace Tiempitod.NET.Configuration.User;

/// <summary>
/// Provides access to user's configuration and custom defined session configurations.
/// </summary>
public class UserConfigurationProvider : DaemonService, IUserConfigurationProvider
{
    private const string UserConfigFileName = "user.conf";
    private const string SessionSectionPrefix = "Session.";
    
    private readonly IAppDirectoryPathProvider _appDirectoryPathProvider;
    private readonly ISessionConfigReader _sessionConfigReader;
    private readonly IFileProvider _fileProvider;

    public IDictionary<string, SessionConfig> SessionConfigs { get; private set; } = new Dictionary<string, SessionConfig>();
    public SessionConfig DefaultSessionConfig { get; private set; }
    
    /// <summary>
    /// Instantiates a new <see cref="UserConfigurationProvider"/>.
    /// </summary>
    /// <param name="logger">Logger to register special events.</param>
    /// <param name="appDirectoryPathProvider">Provider of directory paths.</param>
    /// <param name="sessionConfigReader">Reader of session configurations.</param>
    /// <param name="fileProvider">A provider of files in the user's config directory.</param>
    public UserConfigurationProvider(
        ILogger<UserConfigurationProvider> logger,
        IAppDirectoryPathProvider appDirectoryPathProvider,
        ISessionConfigReader sessionConfigReader,
        [FromKeyedServices(AppDirectoryPathProvider.UserConfigFileProviderKey)] IFileProvider fileProvider) : base(logger)
    {
        _appDirectoryPathProvider = appDirectoryPathProvider;
        _sessionConfigReader = sessionConfigReader;
        _fileProvider = fileProvider;
    }

    protected override void OnStartService()
    {
        CreateUserConfigFile(UserConfigFileName);
        LoadSessionConfigs();
        SetDefaultUserSessionConfig();
    }
    
    /// <summary>
    /// Creates a file in the user's config directory with the given name.
    /// </summary>
    /// <param name="fileName">Name of the file to create.</param>
    private void CreateUserConfigFile(string fileName)
    {
        string path = Path.Combine(_appDirectoryPathProvider.UserConfigDirectoryPath, fileName);

        IFileInfo fileInfo = _fileProvider.GetFileInfo(path);
        
        if (fileInfo.Exists)
            return;
        
        File.Create(path);
        Logger.LogInformation("User config filed was created at {Path}", fileInfo.PhysicalPath);
    }

    /// <summary>
    /// Load session configurations of the user.
    /// </summary>
    private void LoadSessionConfigs()
    {
        IFileInfo userConfigFileInfo = _fileProvider.GetFileInfo(UserConfigFileName);
        SessionConfigs = _sessionConfigReader.ReadSessions(SessionSectionPrefix, userConfigFileInfo);
    }
    
    /// <summary>
    /// Sets the default session configuration of the user.
    /// </summary>
    private void SetDefaultUserSessionConfig()
    {
        if (SessionConfigs.Count < 1)
            return;
        
        if (SessionConfigs.Count < 2)
        {
            DefaultSessionConfig = SessionConfigs.Values.First();
            return;
        }

        if (SessionConfigs.TryGetValue("Default", out SessionConfig sessionConfig))
        {
            DefaultSessionConfig = sessionConfig;
            return;
        }

        DefaultSessionConfig = SessionConfigs.Values.First();
    }
}
