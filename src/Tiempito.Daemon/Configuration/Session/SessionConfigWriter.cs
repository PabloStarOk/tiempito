using Salaros.Configuration;

using Tiempito.Daemon.Common.Interfaces;
using Tiempito.Daemon.Configuration.Session.Enums;
using Tiempito.Daemon.Configuration.Session.Interfaces;
using Tiempito.Daemon.Configuration.Session.Objects;

namespace Tiempito.Daemon.Configuration.Session;

/// <summary>
/// Provides write operations to save <see cref="SessionConfig"/> in user's config file.
/// </summary>
public class SessionConfigWriter : ISessionConfigWriter
{
    private readonly ConfigParser _configParser;
    private readonly ITimeSpanConverter _timeSpanConverter;
    
    /// <summary>
    /// Instantiates a new <see cref="SessionConfigWriter"/>.
    /// </summary>
    /// <param name="configParser">Parser of the user's configuration file.</param>
    /// <param name="timeSpanConverter">A <see cref="ITimeSpanConverter"/> to convert <see cref="TimeSpan"/> to string values.</param>
    public SessionConfigWriter(
        [FromKeyedServices(AppConfigConstants.UserConfigParserServiceKey)] ConfigParser configParser,
        ITimeSpanConverter timeSpanConverter)
    {
        _configParser = configParser;
        _timeSpanConverter = timeSpanConverter;
    }
    
    // TODO: Make method asynchronous.
    public bool Write(string prefixSectionName, SessionConfig sessionConfig)
    {
        string sectionName = prefixSectionName + sessionConfig.Id;
        var targetCycles = sessionConfig.TargetCycles.ToString();
        var delayBetweenTimes = _timeSpanConverter.ConvertToString(sessionConfig.DelayBetweenTimes);
        string focusDuration = _timeSpanConverter.ConvertToString(sessionConfig.FocusDuration);
        string breakDuration = _timeSpanConverter.ConvertToString(sessionConfig.BreakDuration);
        
        bool wasWritten =
            _configParser.SetValue(sectionName, SessionConfigKeyword.TargetCycles.ToString(), targetCycles)
            && _configParser.SetValue(sectionName, SessionConfigKeyword.DelayBetweenTimes.ToString(), delayBetweenTimes)
            && _configParser.SetValue(sectionName, SessionConfigKeyword.FocusDuration.ToString(), focusDuration)
            && _configParser.SetValue(sectionName, SessionConfigKeyword.BreakDuration.ToString(), breakDuration);

        return wasWritten && _configParser.Save();
    }
}
