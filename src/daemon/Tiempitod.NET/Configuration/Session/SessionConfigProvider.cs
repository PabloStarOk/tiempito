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
    private readonly EventHandler _defaultSessionChangedHandler;
    private IDictionary<string, SessionConfig> _sessionConfigs;
    
    public IReadOnlyDictionary<string, SessionConfig> SessionConfigs => _sessionConfigs.AsReadOnly();
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
        _sessionConfigs = new Dictionary<string, SessionConfig>();
        _defaultSessionChangedHandler = (_, _) => SetDefaultUserSessionConfig();
    }

    protected override void OnStartService()
    {
        _userConfigProvider.OnUserConfigChanged += _defaultSessionChangedHandler; 
        LoadSessionConfigs();
        SetDefaultUserSessionConfig();
    }

    protected override void OnStopService()
    { 
        _userConfigProvider.OnUserConfigChanged -= _defaultSessionChangedHandler;
    }

    // TODO: Make method asynchronous.
    public OperationResult SaveSessionConfig(SessionConfig sessionConfig)
    {
        bool wasWritten = _sessionConfigWriter.Write
            (AppConfigConstants.SessionSectionPrefix, sessionConfig);

        if (!wasWritten)
            return new OperationResult(false, "Session configuration was not saved.");
        
        // Add or update
        if (!_sessionConfigs.TryAdd(sessionConfig.Id.ToLower(), sessionConfig)) // TODO: Unify letter case management for sessionId.
            _sessionConfigs[sessionConfig.Id.ToLower()] = sessionConfig; // TODO: Unify letter case management for sessionId.

        return new OperationResult(true, "Session configuration was saved.");
    }

    /// <summary>
    /// Load session configurations of the user.
    /// </summary>
    private void LoadSessionConfigs()
    {
        _sessionConfigs = _sessionConfigReader.ReadSessions(AppConfigConstants.SessionSectionPrefix);
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
