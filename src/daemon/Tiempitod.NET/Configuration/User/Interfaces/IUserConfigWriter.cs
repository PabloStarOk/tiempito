using Tiempitod.NET.Configuration.User.Objects;

namespace Tiempitod.NET.Configuration.User.Interfaces;

/// <summary>
/// Defines a writer for user's configuration.
/// </summary>
public interface IUserConfigWriter
{
    /// <summary>
    /// Saves the user configuration in the user's configuration file.
    /// </summary>
    /// <param name="userConfig">A <see cref="UserConfig"/> to write in the file.</param>
    /// <returns>True if the <see cref="UserConfig"/> was saved, false otherwise.</returns>
    public bool Write(UserConfig userConfig);
}
