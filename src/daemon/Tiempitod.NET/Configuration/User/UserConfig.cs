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

    public UserConfig()
    {
        DefaultSessionId = string.Empty;
    }

    public UserConfig(string defaultSessionId)
    {
        DefaultSessionId = defaultSessionId.ToLower();
    }
}
