using Microsoft.Extensions.FileProviders;
using Salaros.Configuration;

namespace Tiempitod.NET.Configuration.Session;

/// <summary>
/// Provides read and write operations for session configurations.
/// </summary>
public class SessionConfigReader : ISessionConfigReader
{
    /// <summary>
    /// Maps enums <see cref="SessionDurationSymbol"/> to a string representing that unit in lower case.
    /// </summary>
    private readonly static  Dictionary<SessionDurationSymbol, string> TimeUnitSymbolsMap = new()
    {
        { SessionDurationSymbol.Millisecond, "ms" },
        { SessionDurationSymbol.Second, "s" },
        { SessionDurationSymbol.Minute, "m" },
        { SessionDurationSymbol.Hour, "h" }
    };
    
    // TODO: Make method asynchronous.
    public IDictionary<string, SessionConfig> ReadSessions(string prefixSectionName, IFileInfo fileInfo)
    {
        var dictionary = new Dictionary<string, SessionConfig>();

        if (!fileInfo.Exists)
            return dictionary;
        
        var configParser = new ConfigParser(fileInfo.PhysicalPath);

        foreach (ConfigSection configSection in configParser.Sections)
        {
            if (!configSection.SectionName.Contains(prefixSectionName))
                continue;
            
            SessionConfig? sessionConfigNullable = ReadConfigSection(configSection, prefixSectionName);

            if (sessionConfigNullable == null)
                continue;

            SessionConfig sessionConfig = (SessionConfig) sessionConfigNullable;
            dictionary.Add(sessionConfig.Id, sessionConfig);
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
    private static SessionConfig? ReadConfigSection(ConfigSection configSection, string prefixSectionName)
    {
        string id = configSection.SectionName.ToLower().Replace(prefixSectionName.ToLower(), "");
        var targetCyclesStr = string.Empty;
        var focusDurationStr = string.Empty;
        var breakDurationStr = string.Empty;
        
        // Get values from section.
        foreach (IConfigKeyValue configKeyValue in configSection.Keys)
        {
            if (!Enum.TryParse(configKeyValue.Name, out SessionConfigKeyword configKeyword))
                return null;

            switch (configKeyword)
            {
                case SessionConfigKeyword.TargetCycles:
                    targetCyclesStr = configKeyValue.Content;
                    if (string.IsNullOrWhiteSpace(targetCyclesStr))
                        return null;
                    continue;
                
                case SessionConfigKeyword.FocusDuration:
                    focusDurationStr = configKeyValue.Content;
                    if (string.IsNullOrWhiteSpace(focusDurationStr))
                        return null;
                    continue;
                
                case SessionConfigKeyword.BreakDuration:
                    breakDurationStr = configKeyValue.Content;
                    if (string.IsNullOrWhiteSpace(breakDurationStr))
                        return null;
                    break;
                
                default:
                    return null;
            }
        }

        // Parse string values.
        if (!int.TryParse(targetCyclesStr, out int targetCycles))
            return null;
        if (!TryParseDuration(focusDurationStr, out TimeSpan focusDuration))
            return null;
        if (!TryParseDuration(breakDurationStr, out TimeSpan breakDuration))
            return null;
        
        return new SessionConfig(id, targetCycles, focusDuration, breakDuration);
    }
    
    /// <summary>
    /// Tries to extract a time unit symbol from the given string and parses the value.
    /// </summary>
    /// <param name="timeString">String to extract and parse.</param>
    /// <param name="durationSymbol">Time unit symbol to try to extract.</param>
    /// <param name="parsedTime">String parsed to an integer value.</param>
    /// <returns>True if the given duration symbol is correct and the time was parsed successfully, false otherwise.</returns>
    private static bool TryExtractTimeUnit(string timeString, SessionDurationSymbol durationSymbol, out int parsedTime)
    {
        parsedTime = 0;

        string timeStrLowerCase = timeString.ToLower();
        string symbolString = TimeUnitSymbolsMap[durationSymbol].ToLower();

        if (!timeStrLowerCase.EndsWith(symbolString))
            return false;
        
        string amountString = timeStrLowerCase.Replace(symbolString, "");
        return int.TryParse(amountString, out parsedTime);
    }

    /// <summary>
    /// Parse a string as a TimeSpan duration.
    /// </summary>
    /// <param name="timeString">String to parse.</param>
    /// <param name="duration">Parsed value.</param>
    /// <returns>True if the string was parsed successfully, false otherwise.</returns>
    private static bool TryParseDuration(string timeString, out TimeSpan duration)
    {
        duration = TimeSpan.Zero;
        if (TryExtractTimeUnit(timeString, SessionDurationSymbol.Millisecond, out int parsedTime))
        {
            duration = TimeSpan.FromMilliseconds(parsedTime);
            return true;
        }

        if (TryExtractTimeUnit(timeString, SessionDurationSymbol.Second, out parsedTime))
        {
            duration = TimeSpan.FromSeconds(parsedTime);
            return true;
        }

        if (TryExtractTimeUnit(timeString, SessionDurationSymbol.Minute, out parsedTime))
        {
            duration = TimeSpan.FromMinutes(parsedTime);
            return true;
        }

        if (!TryExtractTimeUnit(timeString, SessionDurationSymbol.Minute, out parsedTime))
            return false;

        duration = TimeSpan.FromHours(parsedTime);
        return true;
    }
}
