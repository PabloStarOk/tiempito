using Salaros.Configuration;

using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Config.Sessions;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Config.Enums;

namespace Tiempito.Daemon.Infrastructure.Config.Sessions;

/// <summary>
/// Provides read operations to load <see cref="SessionConfig"/> from user's config file.
/// </summary>
public class SessionConfigReader : ISessionConfigReader
{
    private readonly ConfigParser _configParser;
    private readonly ITimeSpanConverter _timeSpanConverter;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionConfigReader"/> class.
    /// </summary>
    /// <param name="configParser">Parser of the user's configuration file.</param>
    /// <param name="timeSpanConverter">A <see cref="ITimeSpanConverter"/> to convert <see cref="TimeSpan"/> to string values.</param>
    public SessionConfigReader(
        ConfigParser configParser,
        ITimeSpanConverter timeSpanConverter)
    {
        _configParser = configParser;
        _timeSpanConverter = timeSpanConverter;
    }

    // TODO: Make method asynchronous.

    /// <inheritdoc/>
    public IDictionary<string, SessionConfig> ReadSessions(string prefixSectionName)
    {
        var dictionary = new Dictionary<string, SessionConfig>();

        foreach (ConfigSection configSection in _configParser.Sections)
        {
            if (!configSection.SectionName.Contains(prefixSectionName))
            {
                continue;
            }

            SessionConfig? sessionConfig = ReadConfigSection(configSection, prefixSectionName);
            if (sessionConfig is null)
            {
                continue;
            }

            dictionary.TryAdd(sessionConfig.Id, sessionConfig);
        }

        return dictionary;
    }

    // TODO: Make method asynchronous.
    // TODO: Refactor this method.

    /// <summary>
    /// Reads a session configuration section from a .conf file.
    /// </summary>
    /// <param name="configSection">Section of the session configuration.</param>
    /// <param name="prefixSectionName">Prefix of the section's name to remove.</param>
    /// <returns>A <see cref="SessionConfig"/> or null if one of the required fields is not found.</returns>
    private SessionConfig? ReadConfigSection(ConfigSection configSection, string prefixSectionName)
    {
        string id = configSection.SectionName.Replace(prefixSectionName, string.Empty);
        var targetCyclesStr = string.Empty;
        var delayBetweenTimesStr = string.Empty;
        var focusDurationStr = string.Empty;
        var breakDurationStr = string.Empty;

        // Get values from section.
        foreach (IConfigKeyValue configKeyValue in configSection.Keys)
        {
            if (!Enum.TryParse(configKeyValue.Name, out SessionConfigKeyword configKeyword))
            {
                return null;
            }

            switch (configKeyword)
            {
                case SessionConfigKeyword.TargetCycles:
                    targetCyclesStr = configKeyValue.Content;
                    if (string.IsNullOrWhiteSpace(targetCyclesStr))
                    {
                        return null;
                    }

                    continue;

                case SessionConfigKeyword.DelayBetweenTimes: // Optional
                    delayBetweenTimesStr = configKeyValue.Content;
                    continue;

                case SessionConfigKeyword.FocusDuration:
                    focusDurationStr = configKeyValue.Content;
                    if (string.IsNullOrWhiteSpace(focusDurationStr))
                    {
                        return null;
                    }

                    continue;

                case SessionConfigKeyword.BreakDuration:
                    breakDurationStr = configKeyValue.Content;
                    if (string.IsNullOrWhiteSpace(breakDurationStr))
                    {
                        return null;
                    }

                    break;

                default:
                    return null;
            }
        }

        // Parse string values.
        if (!int.TryParse(targetCyclesStr, out int targetCycles))
        {
            return null;
        }

        TimeSpan delayBetweenTimes = TimeSpan.Zero;
        if (!string.IsNullOrWhiteSpace(delayBetweenTimesStr))
        {
            _timeSpanConverter.TryConvert(delayBetweenTimesStr, out delayBetweenTimes);
        }

        if (!_timeSpanConverter.TryConvert(focusDurationStr, out TimeSpan focusDuration))
        {
            return null;
        }

        if (!_timeSpanConverter.TryConvert(breakDurationStr, out TimeSpan breakDuration))
        {
            return null;
        }

        return new SessionConfig(id, targetCycles, delayBetweenTimes, focusDuration, breakDuration);
    }
}
