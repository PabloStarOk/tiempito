using Tiempitod.NET.Configuration.Session.Objects;

namespace Tiempitod.NET.Configuration.Session.Interfaces;

/// <summary>
/// Defines writer of session configurations.
/// </summary>
public interface ISessionConfigWriter
{
    /// <summary>
    /// Writes a session to the user's configuration file.
    /// </summary>
    /// <param name="prefixSectionName">Prefix of the section name in the configuration file.</param>
    /// <param name="sessionConfig">Session to save in the user's configuration file.</param>
    /// <returns>True if the session was saved in the file, false otherwise.</returns>
    public bool Write(string prefixSectionName, SessionConfig sessionConfig);
}
