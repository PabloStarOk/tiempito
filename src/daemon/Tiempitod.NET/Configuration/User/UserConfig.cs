namespace Tiempitod.NET.Configuration.User;

/// <summary>
/// Represents the configuration of the user.
/// </summary>
public struct UserConfig
{
    /// <summary>
    /// The id of the default session to start by the daemon.
    /// </summary>
    public string DefaultSessionId { get; }

    /// <summary>
    /// Instantiates a new <see cref="UserConfig"/>.
    /// </summary>
    public UserConfig()
    {
        DefaultSessionId = string.Empty;
    }

    /// <summary>
    /// Instantiates a new <see cref="UserConfig"/>.
    /// </summary>
    /// <param name="defaultSessionId">Id of the default session of the user.</param>
    public UserConfig(string defaultSessionId)
    {
        DefaultSessionId = defaultSessionId.ToLower();
    }
}
