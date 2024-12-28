namespace Tiempitod.NET.Configuration.User;

/// <summary>
/// Defines a reader of user's configuration
/// </summary>
public interface IUserConfigurationReader
{
    /// <summary>
    /// Reads the user's configuration file to read the user's specific configuration.
    /// </summary>
    /// <returns>A <see cref="UserConfiguration"/> with the configuration.</returns>
    public UserConfiguration Read();
}
