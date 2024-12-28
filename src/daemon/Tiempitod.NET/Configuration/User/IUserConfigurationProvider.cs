namespace Tiempitod.NET.Configuration.User;

/// <summary>
/// Defines a provider of user's configuration.
/// </summary>
public interface IUserConfigurationProvider
{
    /// <summary>
    /// User's configuration.
    /// </summary>
    public UserConfiguration UserConfiguration { get; }
    
    /// <summary>
    /// Saves the given user configuration in the user's configuration file.
    /// </summary>
    /// <param name="userConfig">A <see cref="UserConfiguration"/> to save.</param>
    /// <returns>A <see cref="OperationResult"/> represeting if the operation was completed successfully.</returns>
    public OperationResult SaveUserConfig(UserConfiguration userConfig);
}
