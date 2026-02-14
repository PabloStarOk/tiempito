using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Config.Sessions;
using Tiempito.Daemon.Domain.Config;

namespace Tiempito.Daemon.Infrastructure.Config.Sessions;

/// <summary>
/// Provides read operations to load <see cref="SessionConfig"/> from user's config file.
/// </summary>
public class SessionConfigReader : ISessionConfigReader
{
    private readonly ILogger<SessionConfigReader> _logger;
    private readonly ITimeSpanConverter _timeSpanConverter;
    private readonly IConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionConfigReader"/> class.
    /// </summary>
    /// <param name="logger">Logger to register events.</param>
    /// <param name="timeSpanConverter">A <see cref="ITimeSpanConverter"/> to convert <see cref="TimeSpan"/> to string values.</param>
    /// <param name="config">The configuration source to read session settings from.</param>
    public SessionConfigReader(
        ILogger<SessionConfigReader> logger,
        ITimeSpanConverter timeSpanConverter,
        IConfiguration config)
    {
        _logger = logger;
        _timeSpanConverter = timeSpanConverter;
        _config = config;
    }

    /// <inheritdoc/>
    public IDictionary<string, SessionConfig> ReadSessions(string prefixSectionName)
    {
        var sessionConfigs = new Dictionary<string, SessionConfig>();
        foreach (var kvp in ExtractRawConfigs(prefixSectionName))
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
            sessionConfigs.TryAdd(config.NormalizedId, config);

            _logger.LogInformation("Found session config with ID '{Id}'", config.Id);
            _logger.LogTrace("Session config {Id}: {Config}", config.Id, config);
        }

        return sessionConfigs;
    }

    private Dictionary<string, RawSessionConfig> ExtractRawConfigs(string prefixSectionName)
    {
        Dictionary<string, RawSessionConfig> rawSessionConfigs = [];
        foreach (var configSection in _config.GetSection(prefixSectionName).GetChildren())
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

    internal sealed record RawSessionConfig(
        int TargetCycles,
        string FocusDuration,
        string BreakDuration,
        string? DelayBetweenTimes = null);
}
