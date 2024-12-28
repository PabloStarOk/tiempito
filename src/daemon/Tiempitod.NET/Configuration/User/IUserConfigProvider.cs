namespace Tiempitod.NET.Configuration.User;

/// <summary>
/// Defines a provider of user's configuration.
/// </summary>
public interface IUserConfigProvider
{
    /// <summary>
    /// User's configuration.
    /// </summary>
    public UserConfig UserConfig { get; }
    
    /// <summary>
    /// Saves the given user configuration in the user's configuration file.
    /// </summary>
    /// <param name="userConfig">A <see cref="UserConfig"/> to save.</param>
    /// <returns>A <see cref="OperationResult"/> represeting if the operation was completed successfully.</returns>
    public OperationResult SaveUserConfig(UserConfig userConfig);
}
