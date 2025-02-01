namespace Tiempito.Daemon.Configuration.User.Objects;

/// <summary>
/// Represents a feature that is enabled in the configuration.
/// </summary>
/// <param name="Name">Real name of the feature to save in the config file.</param>
/// <param name="Aliases">Aliases that can be understood by the daemon when parsing.</param>
public record UserConfigFeature(string Name, string[] Aliases);
