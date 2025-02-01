using Tiempitod.NET.Common;
using Tiempitod.NET.Configuration.User;

namespace Tiempitod.NET.Configuration.Session;

/// <summary>
/// Service to manage user's session configurations.
/// </summary>
public class SessionConfigService : Service, ISessionConfigService
{
    private readonly IUserConfigService _userConfigService;
    private readonly ISessionConfigWriter _configWriter;
    private readonly ISessionConfigReader _configReader;
    private Dictionary<string, SessionConfig> _configs;
    
    /// <inheritdoc/>
    public SessionConfig DefaultConfig { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, SessionConfig> Configs => _configs.ToDictionary().AsReadOnly();
    
    /// <summary>
    /// Instantiates a new <see cref="SessionConfigService"/>.
    /// </summary>
    /// <param name="logger">Logger to register special events.</param>
    /// <param name="userConfigService">Service of user's configuration.</param>
    /// <param name="configWriter">Writer of the user's configuration file.</param>
    /// <param name="configReader">Reader of the user's configuration file.</param>
    public SessionConfigService(
        ILogger<SessionConfigService> logger,
        IUserConfigService userConfigService,
        ISessionConfigWriter configWriter,
        ISessionConfigReader configReader) : base(logger)
    {
        _userConfigService = userConfigService;
        _configWriter = configWriter;
        _configReader = configReader;
        _configs = [];
    }

    /// <inheritdoc/>
    public bool TryGetConfigById(string id, out SessionConfig config)
    {
        return _configs.TryGetValue(id.ToLower(), out config);
    }
    
    /// <inheritdoc/>
    public Task<OperationResult> AddConfigAsync(SessionConfig config)
    {
        if (_configs.ContainsKey(config.Id.ToLower()))
        {
            return Task.FromResult(new OperationResult(
                Success: false, 
                Message: $"There's already a session configuration with the same ID \"{config.Id}\"")); 
        }
        
        _configs.Add(config.Id.ToLower(), config);
        bool wasSaved = _configWriter.Write(AppConfigConstants.SessionSectionPrefix, config);
        string message = wasSaved 
            ? "Session configuration added." 
            : "An error occurred while saving the configuration in the file.";
        return Task.FromResult(new OperationResult(wasSaved, message));
    }

    /// <inheritdoc/>
    public Task<OperationResult> ModifyConfigAsync(
        string configId,
        int? targetCycles = null,
        TimeSpan? delayBetweenTimes = null,
        TimeSpan? focusDuration = null,
        TimeSpan? breakDuration = null)
    {
        if (!TryGetConfigById(configId, out SessionConfig config))
        {
            return Task.FromResult(new OperationResult(
                Success: false, 
                Message: $"Session configuration with \"{configId}\" wasn't found.")); 
        }

        SessionConfig modifiedConfig = config with
        {
            TargetCycles = targetCycles ?? config.TargetCycles,
            DelayBetweenTimes = delayBetweenTimes ?? config.DelayBetweenTimes,
            FocusDuration = focusDuration ?? config.FocusDuration,
            BreakDuration = breakDuration ?? config.BreakDuration
        };
        
        bool wasSaved = _configWriter.Write(AppConfigConstants.SessionSectionPrefix, modifiedConfig);
        string message = wasSaved 
            ? "Session configuration modified." 
            : "An error occurred while modifying the configuration of the file.";
        return Task.FromResult(new OperationResult(wasSaved, message));
    }

    /// <inheritdoc/>
    protected override Task<bool> OnStartServiceAsync()
    {
        _configs = _configReader.ReadSessions(AppConfigConstants.SessionSectionPrefix).ToDictionary();
        OnUserConfigChangedHandler(this, EventArgs.Empty); // Set default config.
        _userConfigService.OnConfigChanged += OnUserConfigChangedHandler;
        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    protected override Task<bool> OnStopServiceAsync()
    {
        _userConfigService.OnConfigChanged -= OnUserConfigChangedHandler;
        return Task.FromResult(true);
    }
    
    /// <summary>
    /// Executed when the <see cref="ISessionConfigService"/>
    /// change the user's configuration to set the default
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Empty arguments.</param>
    private void OnUserConfigChangedHandler(object? sender, EventArgs e)
    {
        if (DefaultConfig.Id == _userConfigService.UserConfig.DefaultSessionId)
            return;
        
        switch (_configs.Count)
        {
            case < 1:
                return;
            case < 2:
                DefaultConfig = _configs.Values.First();
                return;
        }

        string configId = _userConfigService.UserConfig.DefaultSessionId; 
        if (_configs.TryGetValue(configId, out SessionConfig sessionConfig))
        {
            DefaultConfig = sessionConfig;
            return;
        }

        DefaultConfig = _configs.Values.First();
    }
}