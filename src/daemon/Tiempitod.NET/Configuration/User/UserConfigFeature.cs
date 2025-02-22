namespace Tiempitod.NET.Configuration.User;

/// <summary>
/// Represents a feature that is enabled in the configuration.
/// </summary>
/// <param name="Name">Real name of the feature to save in the config file.</param>
/// <param name="Aliases">Aliases that can be understood by the daemon when parsing.</param>
public record ConfigFeature(string Name, string[] Aliases);
