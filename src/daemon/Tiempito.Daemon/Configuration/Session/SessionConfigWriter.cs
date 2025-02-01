using Salaros.Configuration;
using System.Globalization;

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
    
    /// <summary>
    /// Instantiates a new <see cref="SessionConfigWriter"/>.
    /// </summary>
    /// <param name="configParser">Parser of the user's configuration file.</param>
    public SessionConfigWriter([FromKeyedServices(AppConfigConstants.UserConfigParserServiceKey)] ConfigParser configParser)
    {
        _configParser = configParser;
    }
    
    // TODO: Make method asynchronous.
    public bool Write(string prefixSectionName, SessionConfig sessionConfig)
    {
        string sectionName = prefixSectionName + sessionConfig.Id;
        var targetCycles = sessionConfig.TargetCycles.ToString();
        var delayBetweenTimes = sessionConfig.DelayBetweenTimes.TotalSeconds.ToString(CultureInfo.InvariantCulture) + "s";
        // BUG: Saving as minutes when are in seconds will cause saving floats which are not gonna be parsed when loading configs again.
        string focusDuration = sessionConfig.FocusDuration.TotalMinutes.ToString(CultureInfo.InvariantCulture) + "m";
        string breakDuration = sessionConfig.BreakDuration.TotalMinutes.ToString(CultureInfo.InvariantCulture) + "m";
        
        bool wasWritten = _configParser.SetValue(sectionName, SessionConfigKeyword.TargetCycles.ToString(), targetCycles)
            && _configParser.SetValue(sectionName, SessionConfigKeyword.DelayBetweenTimes.ToString(), delayBetweenTimes)
            && _configParser.SetValue(sectionName, SessionConfigKeyword.FocusDuration.ToString(), focusDuration)
            && _configParser.SetValue(sectionName, SessionConfigKeyword.BreakDuration.ToString(), breakDuration);

        return wasWritten && _configParser.Save();
    }
}
