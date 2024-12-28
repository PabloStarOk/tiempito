namespace Tiempitod.NET.Configuration.User;

/// <summary>
/// Defines a writer for user's configuration.
/// </summary>
public interface IUserConfigurationWriter
{
    /// <summary>
    /// Saves the user configuration in the user's configuration file.
    /// </summary>
    /// <param name="userConfig">A <see cref="UserConfiguration"/> to write in the file.</param>
    /// <returns>True if the <see cref="UserConfiguration"/> was saved, false otherwise.</returns>
    public bool Write(UserConfiguration userConfig);
}
