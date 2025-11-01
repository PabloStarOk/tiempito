using Salaros.Configuration;

using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Config.Sessions;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Config.Enums;

namespace Tiempito.Daemon.Infrastructure.Config.Sessions;

/// <summary>
/// Provides write operations to save <see cref="SessionConfig"/> in user's config file.
/// </summary>
public class SessionConfigWriter : ISessionConfigWriter
{
    private readonly ConfigParser _configParser;
    private readonly ITimeSpanConverter _timeSpanConverter;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionConfigWriter"/> class.
    /// </summary>
    /// <param name="configParser">Parser of the user's configuration file.</param>
    /// <param name="timeSpanConverter">A <see cref="ITimeSpanConverter"/> to convert <see cref="TimeSpan"/> to string values.</param>
    public SessionConfigWriter(
        ConfigParser configParser,
        ITimeSpanConverter timeSpanConverter)
    {
        _configParser = configParser;
        _timeSpanConverter = timeSpanConverter;
    }

    // TODO: Make method asynchronous.

    /// <inheritdoc/>
    public bool Write(string prefixSectionName, SessionConfig sessionConfig)
    {
        string sectionName = prefixSectionName + sessionConfig.Id;
        var targetCycles = sessionConfig.TargetCycles.ToString();
        var delayBetweenTimes = _timeSpanConverter.Format(sessionConfig.DelayBetweenTimes);
        string focusDuration = _timeSpanConverter.Format(sessionConfig.FocusDuration);
        string breakDuration = _timeSpanConverter.Format(sessionConfig.BreakDuration);

        bool wasWritten =
            _configParser.SetValue(sectionName, nameof(SessionConfigKeyword.TargetCycles), targetCycles)
            && _configParser.SetValue(sectionName, nameof(SessionConfigKeyword.DelayBetweenTimes), delayBetweenTimes)
            && _configParser.SetValue(sectionName, nameof(SessionConfigKeyword.FocusDuration), focusDuration)
            && _configParser.SetValue(sectionName, nameof(SessionConfigKeyword.BreakDuration), breakDuration);

        return wasWritten && _configParser.Save();
    }
}
