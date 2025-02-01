using Tiempitod.NET.Configuration.Session.Objects;

namespace Tiempitod.NET.Configuration.Session.Interfaces;

/// <summary>
/// Defines a class to read configurations.
/// </summary>
public interface ISessionConfigReader
{
    /// <summary>
    /// Reads all session configurations from a file.
    /// </summary>
    /// <param name="prefixSectionName">Prefix of the section names to identify session configurations.</param>
    /// <returns>An <see cref="IDictionary{TKey,TValue}"/> of <see cref="SessionConfig"/> containing all parsed sections of the config file, each key is the id of the session.</returns>
    public IDictionary<string, SessionConfig> ReadSessions(string prefixSectionName);
}
