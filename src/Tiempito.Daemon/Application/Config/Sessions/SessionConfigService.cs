using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Options;

using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Shared;

namespace Tiempito.Daemon.Application.Config.Sessions;

/// <summary>
/// Service to manage user's session configurations.
/// </summary>
public sealed class SessionConfigService : ISessionConfigService, IHostedService, IDisposable
{
    private readonly ILogger<SessionConfigService> _logger;
    private readonly ISessionConfigWriter _configWriter;
    private readonly IOptionsMonitor<IDictionary<string, SessionConfig>> _sessionConfigsMonitor;
    private readonly IOptionsMonitor<UserConfig> _userConfigMonitor;
    private IDisposable? _userConfigChangesListener;
    private IDisposable? _configsChangesListener;

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
    /// <param name="sessionConfigsMonitor">Monitors changes to the session configurations.</param>
    /// <param name="userConfigMonitor">Monitors changes to the user's configuration.</param>
    public SessionConfigService(
        ILogger<SessionConfigService> logger,
        ISessionConfigWriter configWriter,
        IOptionsMonitor<IDictionary<string, SessionConfig>> sessionConfigsMonitor,
        IOptionsMonitor<UserConfig> userConfigMonitor)
    {
        _logger = logger;
        _configWriter = configWriter;
        _sessionConfigsMonitor = sessionConfigsMonitor;
        _userConfigMonitor = userConfigMonitor;
    }

    /// <inheritdoc/>
    public bool TryGetConfigById(string id, [NotNullWhen(true)] out SessionConfig? config)
    {
        var normalizedId = SessionConfig.NormalizeId(id);
        return _sessionConfigsMonitor.CurrentValue.TryGetValue(normalizedId, out config);
    }

    /// <inheritdoc/>
    public Task<OperationResult> AddConfigAsync(SessionConfig config)
    {
        if (_sessionConfigsMonitor.CurrentValue.ContainsKey(config.NormalizedId))
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
        if (!TryGetConfigById(configId, out SessionConfig? currentConfig))
        {
            return Task.FromResult(new OperationResult(
                Success: false,
                Message: $"Session configuration with \"{configId}\" wasn't found."));
        }

        SessionConfig modifiedConfig = currentConfig with
        {
            TargetCycles = targetCycles ?? currentConfig.TargetCycles,
            DelayBetweenTimes = delayBetweenTimes ?? currentConfig.DelayBetweenTimes,
            FocusDuration = focusDuration ?? currentConfig.FocusDuration,
            BreakDuration = breakDuration ?? currentConfig.BreakDuration
        };

        bool wasSaved = _configWriter.Write(AppConfigConstants.SessionSectionPrefix, modifiedConfig);
        string message = wasSaved
            ? "Session configuration modified."
            : "An error occurred while modifying the configuration of the file.";

        if (wasSaved)
        {
            _logger.LogTrace("Session configuration has been modified: {Config}", currentConfig);
        }

        return Task.FromResult(new OperationResult(wasSaved, message));
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        UpdateDefaultSessionConfig(_userConfigMonitor.CurrentValue, null);
        _userConfigChangesListener = _userConfigMonitor.OnChange(UpdateDefaultSessionConfig);
        _configsChangesListener = _sessionConfigsMonitor.OnChange(ApplyDefaultConfigModifications);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _userConfigChangesListener?.Dispose();
        _configsChangesListener?.Dispose();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _userConfigChangesListener?.Dispose();
        _configsChangesListener?.Dispose();
    }

    private void UpdateDefaultSessionConfig(UserConfig userConfig, string? _)
    {
        var configs = _sessionConfigsMonitor.CurrentValue;
        if (DefaultConfig.NormalizedId.Equals(userConfig.DefaultConfigId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        switch (configs.Count)
        {
            case < 1:
                return;
            case < 2:
                DefaultConfig = configs.Values.First();
                return;
        }

        var newConfigId = userConfig.DefaultConfigId;
        if (newConfigId is not null && configs.TryGetValue(newConfigId, out SessionConfig? sessionConfig))
        {
            DefaultConfig = sessionConfig;
            return;
        }

        DefaultConfig = configs.Values.First();
    }

    private void ApplyDefaultConfigModifications(IDictionary<string, SessionConfig> configs, string? _)
    {
        var defaultConfigId = _userConfigMonitor.CurrentValue.DefaultConfigId;
        if (string.IsNullOrWhiteSpace(defaultConfigId))
        {
            return;
        }

        if (!configs.TryGetValue(defaultConfigId, out SessionConfig? defaultConfig))
        {
            return;
        }

        if (defaultConfig == DefaultConfig)
        {
            return;
        }

        DefaultConfig = defaultConfig;
        _logger.LogDebug("Default session configuration modifications have been detected.");
    }
}