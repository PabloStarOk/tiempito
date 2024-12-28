namespace Tiempitod.NET.Configuration.Session;

/// <summary>
/// Defines a provider for user's configuration.
/// </summary>
public interface ISessionConfigurationProvider
{
    /// <summary>
    /// Default and custom session configurations of the user.
    /// </summary>
    public IDictionary<string, SessionConfig> SessionConfigs { get; }
    
    /// <summary>
    /// Default session defined by default or by the user.
    /// </summary>
    public SessionConfig DefaultSessionConfig { get; }
}
