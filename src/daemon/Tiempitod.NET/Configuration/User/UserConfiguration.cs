namespace Tiempitod.NET.Configuration.User;

/// <summary>
/// Represents the configuration of the user.
/// </summary>
public struct UserConfiguration
{
    /// <summary>
    /// The id of the default session to start by the daemon.
    /// </summary>
    public string DefaultSessionId { get; }

    public UserConfiguration()
    {
        DefaultSessionId = string.Empty;
    }

    public UserConfiguration(string defaultSessionId)
    {
        DefaultSessionId = defaultSessionId.ToLower();
    }
}
