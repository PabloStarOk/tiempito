namespace Tiempitod.NET.Configuration.User;

/// <summary>
/// Represents the configuration of the user.
/// </summary>
public struct UserConfiguration
{
    /// <summary>
    /// The id of the default session to start by the daemon.
    /// </summary>
    public string DefaultSession { get; }

    public UserConfiguration()
    {
        DefaultSession = string.Empty;
    }

    public UserConfiguration(string defaultSession)
    {
        DefaultSession = defaultSession;
    }
}
