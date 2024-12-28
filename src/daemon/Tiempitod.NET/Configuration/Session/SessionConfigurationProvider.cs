using Tiempitod.NET.Configuration.User;

namespace Tiempitod.NET.Configuration.Session;

/// <summary>
/// Provides access to user's configuration and custom defined session configurations.
/// </summary>
public class SessionConfigurationProvider : DaemonService, ISessionConfigurationProvider
{
    private readonly IUserConfigurationProvider _userConfigurationProvider;
    private readonly ISessionConfigReader _sessionConfigReader;
    private readonly ISessionConfigWriter _sessionConfigWriter;

    public IDictionary<string, SessionConfig> SessionConfigs { get; private set; } = new Dictionary<string, SessionConfig>();
    public SessionConfig DefaultSessionConfig { get; private set; }
    
    /// <summary>
    /// Instantiates a new <see cref="SessionConfigurationProvider"/>.
    /// </summary>
    /// <param name="logger">Logger to register special events.</param>
    /// <param name="userConfigurationProvider">Provider of user's configuration.</param>
    /// <param name="sessionConfigReader">Reader of session configurations.</param>
    /// <param name="sessionConfigWriter">Writer of session configurations.</param>
    public SessionConfigurationProvider(
        ILogger<SessionConfigurationProvider> logger,
        IUserConfigurationProvider userConfigurationProvider,
        ISessionConfigReader sessionConfigReader,
        ISessionConfigWriter sessionConfigWriter) : base(logger)
    {
        _userConfigurationProvider = userConfigurationProvider;
        _sessionConfigReader = sessionConfigReader;
        _sessionConfigWriter = sessionConfigWriter;
    }

    protected override void OnStartService()
    {
        LoadSessionConfigs();
        SetDefaultUserSessionConfig();
    }

    // TODO: Make method asynchronous.
    public OperationResult SaveSessionConfig(SessionConfig sessionConfig)
    {
        return  _sessionConfigWriter.Write
            (AppConfigConstants.SessionSectionPrefix, sessionConfig) ?
            new OperationResult(true, "Session configuration was saved.") :
            new OperationResult(true, "Session configuration was not saved.");
    }

    /// <summary>
    /// Load session configurations of the user.
    /// </summary>
    private void LoadSessionConfigs()
    {
        SessionConfigs = _sessionConfigReader.ReadSessions(AppConfigConstants.SessionSectionPrefix);
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
        
        string userDefaultSession = _userConfigurationProvider.UserConfiguration.DefaultSessionId;
        if (SessionConfigs.TryGetValue(userDefaultSession, out SessionConfig sessionConfig))
        {
            DefaultSessionConfig = sessionConfig;
            return;
        }

        DefaultSessionConfig = SessionConfigs.Values.First();
    }
}
