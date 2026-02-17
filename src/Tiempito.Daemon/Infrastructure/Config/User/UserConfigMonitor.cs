using System.Collections.Immutable;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Domain.Config;
using Tiempito.IPC.Models.Enums;

namespace Tiempito.Daemon.Infrastructure.Config.User;

/// <summary>
/// Monitors and provides access to the current <see cref="UserConfig"/> instance,
/// supporting change notifications and automatic reloads when the configuration changes.
/// </summary>
internal sealed class UserConfigMonitor : IOptionsMonitor<UserConfig>, IDisposable
{
    /// <inheritdoc/>
    public UserConfig CurrentValue { get; private set; }

    private event Action<UserConfig, string?>? Changed;

    private readonly ILogger<UserConfigMonitor> _logger;
    private readonly IConfiguration _config;
    private readonly IDisposable _changesListener;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConfigMonitor"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging configuration changes.</param>
    /// <param name="config">The configuration source to monitor for user configuration changes.</param>
    public UserConfigMonitor(ILogger<UserConfigMonitor> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
        _changesListener = ChangeToken.OnChange(_config.GetReloadToken, OnConfigChanged);
        CurrentValue = ParseUserConfig();
    }

    /// <inheritdoc/>
    public UserConfig Get(string? name)
    {
        return CurrentValue;
    }

    /// <inheritdoc/>
    public IDisposable OnChange(Action<UserConfig, string?> listener)
    {
        Changed += listener;
        return new DisposableListener(() => Changed -= listener);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _changesListener.Dispose();
    }

    private void OnConfigChanged()
    {
        var newConfig = ParseUserConfig();
        if (newConfig.DefaultConfigId == CurrentValue.DefaultConfigId
            && newConfig.EnabledFeatures.SequenceEqual(CurrentValue.EnabledFeatures))
        {
            return;
        }

        CurrentValue = newConfig;
        Changed?.Invoke(CurrentValue, null);
        _logger.LogTrace(
            "User configuration reloaded: Default Config Id: {DefaultConfigId}, Enabled Features: [{EnabledFeatures}]",
            CurrentValue.DefaultConfigId,
            string.Join(", ", CurrentValue.EnabledFeatures));
    }

    private UserConfig ParseUserConfig()
    {
        var userConfig = _config.GetRequiredSection(AppConfigConstants.UserSectionName);
        var enabledFeaturesValue = userConfig.GetValue<string>(nameof(UserConfig.EnabledFeatures));
        var enabledFeatureStrings = enabledFeaturesValue?.Split(',').Select(f => f.Trim()) ?? [];
        List<UserFeature> enabledFeatures = [];
        foreach (string featString in enabledFeatureStrings)
        {
            if (!Enum.TryParse(featString, ignoreCase: true, out UserFeature feature))
            {
                continue;
            }

            enabledFeatures.Add(feature);
        }

        var defaultSessionId = userConfig.GetValue<string>(nameof(UserConfig.DefaultConfigId));
        return new UserConfig(defaultSessionId, enabledFeatures.ToImmutableHashSet());
    }

    private sealed class DisposableListener(Action action)
        : IDisposable
    {
        public void Dispose() => action();
    }
}