using Tiempitod.NET.Common;
using Tiempitod.NET.Configuration.User;

namespace Tiempitod.NET.Configuration.Session;

/// <summary>
/// Provides access to user's configuration and custom defined session configurations.
/// </summary>
public class SessionConfigProvider : Service, ISessionConfigProvider
{
    private readonly IUserConfigService _userConfigService;
    private readonly ISessionConfigReader _sessionConfigReader;
    private readonly ISessionConfigWriter _sessionConfigWriter;
    private IDictionary<string, SessionConfig> _sessionConfigs;
    
    public IReadOnlyDictionary<string, SessionConfig> SessionConfigs => _sessionConfigs.AsReadOnly();
    public SessionConfig DefaultSessionConfig { get; private set; }
    
    /// <summary>
    /// Instantiates a new <see cref="SessionConfigProvider"/>.
    /// </summary>
    /// <param name="logger">Logger to register special events.</param>
    /// <param name="userConfigService">Service of user's configuration.</param>
    /// <param name="sessionConfigReader">Reader of session configurations.</param>
    /// <param name="sessionConfigWriter">Writer of session configurations.</param>
    public SessionConfigProvider(
        ILogger<SessionConfigProvider> logger,
        IUserConfigService userConfigService,
        ISessionConfigReader sessionConfigReader,
        ISessionConfigWriter sessionConfigWriter) : base(logger)
    {
        _userConfigService = userConfigService;
        _sessionConfigReader = sessionConfigReader;
        _sessionConfigWriter = sessionConfigWriter;
        _sessionConfigs = new Dictionary<string, SessionConfig>();
    }

    protected override Task<bool> OnStartServiceAsync()
    {
        _userConfigService.OnConfigChanged += SetDefaultSessionConfig; 
        LoadSessionConfigs();
        SetDefaultSessionConfig(null, EventArgs.Empty);
        return Task.FromResult(true);
    }

    protected override Task<bool> OnStopServiceAsync()
    { 
        _userConfigService.OnConfigChanged -= SetDefaultSessionConfig;
        return Task.FromResult(true);
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
    private void SetDefaultSessionConfig(object? sender, EventArgs e)
    {
        if (SessionConfigs.Count < 1)
            return;
        
        if (SessionConfigs.Count < 2)
        {
            DefaultSessionConfig = SessionConfigs.Values.First();
            return;
        }
        
        string configId = _userConfigService.UserConfig.DefaultSessionId; 
        if (SessionConfigs.TryGetValue(configId, out SessionConfig sessionConfig))
        {
            DefaultSessionConfig = sessionConfig;
            return;
        }

        DefaultSessionConfig = SessionConfigs.Values.First();
    }
}
