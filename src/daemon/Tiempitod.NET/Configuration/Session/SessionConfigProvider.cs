using Tiempitod.NET.Configuration.User;

namespace Tiempitod.NET.Configuration.Session;

/// <summary>
/// Provides access to user's configuration and custom defined session configurations.
/// </summary>
public class SessionConfigProvider : DaemonService, ISessionConfigProvider
{
    private readonly IUserConfigProvider _userConfigProvider;
    private readonly ISessionConfigReader _sessionConfigReader;
    private readonly ISessionConfigWriter _sessionConfigWriter;

    public IDictionary<string, SessionConfig> SessionConfigs { get; private set; } = new Dictionary<string, SessionConfig>();
    public SessionConfig DefaultSessionConfig { get; private set; }
    
    /// <summary>
    /// Instantiates a new <see cref="SessionConfigProvider"/>.
    /// </summary>
    /// <param name="logger">Logger to register special events.</param>
    /// <param name="userConfigProvider">Provider of user's configuration.</param>
    /// <param name="sessionConfigReader">Reader of session configurations.</param>
    /// <param name="sessionConfigWriter">Writer of session configurations.</param>
    public SessionConfigProvider(
        ILogger<SessionConfigProvider> logger,
        IUserConfigProvider userConfigProvider,
        ISessionConfigReader sessionConfigReader,
        ISessionConfigWriter sessionConfigWriter) : base(logger)
    {
        _userConfigProvider = userConfigProvider;
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
        bool wasWritten = _sessionConfigWriter.Write
            (AppConfigConstants.SessionSectionPrefix, sessionConfig);
        
        if (wasWritten)
            SessionConfigs.TryAdd(sessionConfig.Id, sessionConfig);

        return wasWritten ?
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
        
        string userDefaultSession = _userConfigProvider.UserConfig.DefaultSessionId;
        if (SessionConfigs.TryGetValue(userDefaultSession, out SessionConfig sessionConfig))
        {
            DefaultSessionConfig = sessionConfig;
            return;
        }

        DefaultSessionConfig = SessionConfigs.Values.First();
    }
}
