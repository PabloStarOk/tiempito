namespace Tiempitod.NET.Configuration.Session;

/// <summary>
/// Defines a provider for user's configuration.
/// </summary>
public interface ISessionConfigProvider
{
    /// <summary>
    /// Default and custom session configurations of the user.
    /// </summary>
    public IDictionary<string, SessionConfig> SessionConfigs { get; }
    
    /// <summary>
    /// Default session defined by default or by the user.
    /// </summary>
    public SessionConfig DefaultSessionConfig { get; }

    /// <summary>
    /// Saves the given session in the user's config file.
    /// </summary>
    /// <param name="sessionConfig">Session to save.</param>
    /// <returns>An <see cref="OperationResult"/> representing the result of the operation.</returns>
    public OperationResult SaveSessionConfig(SessionConfig sessionConfig);
}
