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
}
