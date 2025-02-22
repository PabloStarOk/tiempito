namespace Tiempitod.NET.Configuration.User;

/// <summary>
/// Defines a reader of user's configuration
/// </summary>
public interface IUserConfigReader
{
    /// <summary>
    /// Reads the user's configuration file to read the user's specific configuration.
    /// </summary>
    /// <returns>A <see cref="UserConfig"/> with the configuration.</returns>
    public UserConfig Read();
}
