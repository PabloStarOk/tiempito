using System.Collections.Immutable;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Domain.Config;

namespace Tiempito.Daemon.Infrastructure.Config.Sessions;

/// <summary>
/// Monitors and manages <see cref="SessionConfig"/> changes, providing the current set of session configs
/// and notifying listeners when changes occur.
/// </summary>
internal sealed class SessionConfigsMonitor : IOptionsMonitor<IDictionary<string, SessionConfig>>, IDisposable
{
    /// <inheritdoc/>
    public IDictionary<string, SessionConfig> CurrentValue { get; private set; }

    private readonly ILogger<SessionConfigsMonitor> _logger;
    private readonly IConfiguration _config;
    private readonly ITimeSpanConverter _timeSpanConverter;
    private readonly IDisposable _changesSubscription;

    private event Action<IDictionary<string, SessionConfig>, string?>? Changed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionConfigsMonitor"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging configuration events.</param>
    /// <param name="config">The configuration source to monitor for session configs.</param>
    /// <param name="timeSpanConverter">The converter for parsing time span values from configuration.</param>
    public SessionConfigsMonitor(
        ILogger<SessionConfigsMonitor> logger,
        IConfiguration config,
        ITimeSpanConverter timeSpanConverter)
    {
        _logger = logger;
        _config = config;
        _timeSpanConverter = timeSpanConverter;
        _changesSubscription = ChangeToken.OnChange(_config.GetReloadToken, OnConfigChanged);
        CurrentValue = ParseSessionConfigs();
    }

    /// <inheritdoc/>
    public IDictionary<string, SessionConfig> Get(string? name)
    {
        return CurrentValue;
    }

    /// <inheritdoc/>
    public IDisposable OnChange(Action<IDictionary<string, SessionConfig>, string?> listener)
    {
        Changed += listener;
        return new DisposableListener(() => Changed -= listener);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _changesSubscription.Dispose();
    }

    internal sealed record RawSessionConfig(
        int TargetCycles,
        string FocusDuration,
        string BreakDuration,
        string? DelayBetweenTimes = null);

    private void OnConfigChanged()
    {
        var newConfigs = ParseSessionConfigs();
        if (newConfigs.Values.SequenceEqual(CurrentValue.Values))
        {
            return;
        }

        CurrentValue = newConfigs;
        Changed?.Invoke(CurrentValue, null);
        _logger.LogDebug("Session configurations reloaded");
    }

    private ImmutableDictionary<string, SessionConfig> ParseSessionConfigs()
    {
        Dictionary<string, SessionConfig> configs = [];
        foreach (var kvp in ExtractRawConfigs())
        {
            _timeSpanConverter.TryConvert(kvp.Value.DelayBetweenTimes ?? string.Empty, out TimeSpan delayDuration);
            if (!_timeSpanConverter.TryConvert(kvp.Value.FocusDuration, out TimeSpan focusDuration)
                || !_timeSpanConverter.TryConvert(kvp.Value.BreakDuration, out TimeSpan breakDuration))
            {
                continue;
            }

            var config = new SessionConfig(
                kvp.Key,
                kvp.Value.TargetCycles,
                delayDuration,
                focusDuration,
                breakDuration);
            configs.TryAdd(config.NormalizedId, config);

            _logger.LogInformation("Found session config with ID '{Id}'", config.Id);
            _logger.LogTrace("Session config {Id}: {Config}", config.Id, config);
        }

        return configs.ToImmutableDictionary();
    }

    private Dictionary<string, RawSessionConfig> ExtractRawConfigs()
    {
        Dictionary<string, RawSessionConfig> rawSessionConfigs = [];
        foreach (var configSection in _config.GetSection(AppConfigConstants.SessionSectionPrefix).GetChildren())
        {
            RawSessionConfig? rawConfig;
            try
            {
                rawConfig = configSection.Get<RawSessionConfig>();
            }
            catch (InvalidOperationException)
            {
                _logger.LogError(
                    "Could not read session config '{SectionName}', ensure it specifies the required properties correctly.",
                    configSection.Key);
                continue;
            }

            if (rawConfig is null)
            {
                _logger.LogError(
                    "Could not read session config '{SectionName}', ensure it specifies the required properties correctly.",
                    configSection.Key);
                continue;
            }

            rawSessionConfigs.TryAdd(configSection.Key, rawConfig);
        }

        return rawSessionConfigs;
    }

    private sealed class DisposableListener(Action action)
        : IDisposable
    {
        public void Dispose() => action();
    }
}