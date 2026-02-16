using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Options;

using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Shared;

namespace Tiempito.Daemon.Application.Config.Sessions;

/// <summary>
/// Service to manage user's session configurations.
/// </summary>
public class SessionConfigService : ISessionConfigService, IHostedService
{
    private readonly ILogger<SessionConfigService> _logger;
    private readonly ISessionConfigWriter _configWriter;
    private readonly ISessionConfigReader _configReader;
    private readonly IOptionsMonitor<UserConfig> _userConfigMonitor;
    private IDisposable? _userConfigChangesListener;
    private Dictionary<string, SessionConfig> _configs;

    /// <inheritdoc/>
    public SessionConfig DefaultConfig { get; private set; } = new (
        Id: "Default",
        TargetCycles: 4,
        DelayBetweenTimes: TimeSpan.FromSeconds(10),
        FocusDuration: TimeSpan.FromMinutes(25),
        BreakDuration: TimeSpan.FromMinutes(5));

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionConfigService"/> class.
    /// </summary>
    /// <param name="logger">Logger to register events.</param>
    /// <param name="configWriter">Writer of the user's configuration file.</param>
    /// <param name="configReader">Reader of the user's configuration file.</param>
    /// <param name="userConfigMonitor">Monitors changes to the user's configuration.</param>
    public SessionConfigService(
        ILogger<SessionConfigService> logger,
        ISessionConfigWriter configWriter,
        ISessionConfigReader configReader,
        IOptionsMonitor<UserConfig> userConfigMonitor)
    {
        _logger = logger;
        _configWriter = configWriter;
        _configReader = configReader;
        _configs = [];
        _userConfigMonitor = userConfigMonitor;
    }

    /// <inheritdoc/>
    public bool TryGetConfigById(string id, [NotNullWhen(true)] out SessionConfig? config)
    {
        var normalizedId = SessionConfig.NormalizeId(id);
        return _configs.TryGetValue(normalizedId, out config);
    }

    /// <inheritdoc/>
    public Task<OperationResult> AddConfigAsync(SessionConfig config)
    {
        if (!_configs.TryAdd(config.NormalizedId, config))
        {
            return Task.FromResult(new OperationResult(
                Success: false,
                Message: $"There's already a session configuration with the same ID \"{config.NormalizedId}\""));
        }

        bool wasSaved = _configWriter.Write(AppConfigConstants.SessionSectionPrefix, config);
        string message = wasSaved
            ? "Session configuration added."
            : "An error occurred while saving the configuration in the file.";

        if (wasSaved)
        {
            _logger.LogTrace("Session configuration has been added: {Config}", config);
        }
        else
        {
            _configs.Remove(config.NormalizedId);
        }

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
        var normalizedId = SessionConfig.NormalizeId(configId);
        if (!TryGetConfigById(normalizedId, out SessionConfig? currentConfig))
        {
            return Task.FromResult(new OperationResult(
                Success: false,
                Message: $"Session configuration with \"{normalizedId}\" wasn't found."));
        }

        SessionConfig modifiedConfig = currentConfig with
        {
            TargetCycles = targetCycles ?? currentConfig.TargetCycles,
            DelayBetweenTimes = delayBetweenTimes ?? currentConfig.DelayBetweenTimes,
            FocusDuration = focusDuration ?? currentConfig.FocusDuration,
            BreakDuration = breakDuration ?? currentConfig.BreakDuration
        };

        _configs[normalizedId] = modifiedConfig;
        if (_userConfigMonitor.CurrentValue.DefaultConfigId == modifiedConfig.NormalizedId)
        {
            DefaultConfig = modifiedConfig;
        }

        bool wasSaved = _configWriter.Write(AppConfigConstants.SessionSectionPrefix, modifiedConfig);
        string message = wasSaved
            ? "Session configuration modified."
            : "An error occurred while modifying the configuration of the file.";

        if (wasSaved)
        {
            _logger.LogTrace("Session configuration has been modified: {Config}", currentConfig);
        }
        else
        {
            _configs[normalizedId] = currentConfig;
        }

        return Task.FromResult(new OperationResult(wasSaved, message));
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _configs = _configReader.ReadSessions(AppConfigConstants.SessionSectionPrefix).ToDictionary();
        UpdateDefaultSessionConfig(_userConfigMonitor.CurrentValue, null);
        _userConfigChangesListener = _userConfigMonitor.OnChange(UpdateDefaultSessionConfig);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _userConfigChangesListener?.Dispose();
        return Task.CompletedTask;
    }

    private void UpdateDefaultSessionConfig(UserConfig userConfig, string? _)
    {
        if (DefaultConfig.NormalizedId.Equals(userConfig.DefaultConfigId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        switch (_configs.Count)
        {
            case < 1:
                return;
            case < 2:
                DefaultConfig = _configs.Values.First();
                return;
        }

        var newConfigId = userConfig.DefaultConfigId;
        if (newConfigId is not null && _configs.TryGetValue(newConfigId, out SessionConfig? sessionConfig))
        {
            DefaultConfig = sessionConfig;
            return;
        }

        DefaultConfig = _configs.Values.First();
    }
}