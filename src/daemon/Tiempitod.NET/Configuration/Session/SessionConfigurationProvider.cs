using Microsoft.Extensions.FileProviders;
using Tiempitod.NET.Configuration.AppDirectory;

namespace Tiempitod.NET.Configuration.Session;

/// <summary>
/// Provides access to user's configuration and custom defined session configurations.
/// </summary>
public class SessionConfigurationProvider : DaemonService, ISessionConfigurationProvider
{
    private const string UserConfigFileName = "user.conf";
    private const string SessionSectionPrefix = "Session.";
    
    private readonly ISessionConfigReader _sessionConfigReader;
    private readonly ISessionConfigWriter _sessionConfigWriter;
    private readonly IFileProvider _userDirectoryFileProvider;

    public IDictionary<string, SessionConfig> SessionConfigs { get; private set; } = new Dictionary<string, SessionConfig>();
    public SessionConfig DefaultSessionConfig { get; private set; }
    
    /// <summary>
    /// Instantiates a new <see cref="SessionConfigurationProvider"/>.
    /// </summary>
    /// <param name="logger">Logger to register special events.</param>
    /// <param name="sessionConfigReader">Reader of session configurations.</param>
    /// <param name="sessionConfigWriter">Writer of session configurations.</param>
    /// <param name="userDirectoryFileProvider">A provider of files in the user's config directory.</param>
    public SessionConfigurationProvider(
        ILogger<SessionConfigurationProvider> logger,
        ISessionConfigReader sessionConfigReader,
        ISessionConfigWriter sessionConfigWriter,
        [FromKeyedServices(AppDirectoryPathProvider.UserConfigFileProviderKey)] IFileProvider userDirectoryFileProvider) : base(logger)
    {
        _sessionConfigReader = sessionConfigReader;
        _sessionConfigWriter = sessionConfigWriter;
        _userDirectoryFileProvider = userDirectoryFileProvider;
    }

    protected override void OnStartService()
    {
        CreateUserConfigFile(UserConfigFileName);
        LoadSessionConfigs();
        SetDefaultUserSessionConfig();
    }

    // TODO: Make method asynchronous.
    public OperationResult SaveSessionConfig(SessionConfig sessionConfig)
    {
        IFileInfo userConfigFileInfo = _userDirectoryFileProvider.GetFileInfo(UserConfigFileName);
        
        if (!userConfigFileInfo.Exists)
            CreateUserConfigFile(UserConfigFileName);
        
        return  _sessionConfigWriter.Write
            (SessionSectionPrefix, sessionConfig) ?
            new OperationResult(true, "Session configuration was saved.") :
            new OperationResult(true, "Session configuration was not saved.");
    }

    /// <summary>
    /// Creates a file in the user's config directory with the given name.
    /// </summary>
    /// <param name="fileName">Name of the file to create.</param>
    private void CreateUserConfigFile(string fileName)
    {
        IFileInfo fileInfo = _userDirectoryFileProvider.GetFileInfo(fileName);
        
        if (fileInfo.Exists)
            return;
        
        File.Create(fileInfo.PhysicalPath);
        Logger.LogInformation("User config filed was created at {Path}", fileInfo.PhysicalPath);
    }

    /// <summary>
    /// Load session configurations of the user.
    /// </summary>
    private void LoadSessionConfigs()
    {
        SessionConfigs = _sessionConfigReader.ReadSessions(SessionSectionPrefix);
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
