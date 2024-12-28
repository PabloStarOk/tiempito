using Salaros.Configuration;
using System.Globalization;

namespace Tiempitod.NET.Configuration.Session;

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
        string focusDuration = sessionConfig.FocusDuration.TotalMinutes.ToString(CultureInfo.InvariantCulture) + "m";
        string breakDuration = sessionConfig.BreakDuration.TotalMinutes.ToString(CultureInfo.InvariantCulture) + "m";
        
        bool wasWritten = _configParser.SetValue(sectionName, "TargetCycles", targetCycles)
            && _configParser.SetValue(sectionName, "FocusDuration", focusDuration)
            && _configParser.SetValue(sectionName, "BreakDuration", breakDuration);

        return wasWritten && _configParser.Save();
    }
}
