using System.Collections.Immutable;

namespace Tiempitod.NET.Configuration.User;

// TODO: Make struct readonly.
/// <summary>
/// Represents the configuration of the user.
/// </summary>
public struct UserConfig
{
    private readonly List<string> _enabledFeatures = [];
    
    /// <summary>
    /// The id of the default session to start by the daemon.
    /// </summary>
    public string DefaultSessionId { get; }
    public IReadOnlyList<string> EnabledFeatures => _enabledFeatures;
    
    /// <summary>
    /// Allowed features that can be understood by the daemon.
    /// </summary>
    public static ImmutableArray<ConfigFeature> AllowedFeatures { get; }  =
    [
        ..new ConfigFeature[]
        {
            new(Name: "notification", Aliases: ["nc"])
        }
    ];

    /// <summary>
    /// Instantiates a new <see cref="UserConfig"/>.
    /// </summary>
    public UserConfig()
    {
        DefaultSessionId = string.Empty;
    }

    /// <summary>
    /// Instantiates a new <see cref="UserConfig"/>.
    /// </summary>
    /// <param name="defaultSessionId">ID of the default session of the user.</param>
    public UserConfig(string defaultSessionId)
    {
        DefaultSessionId = defaultSessionId.ToLower();
    }

    /// <summary>
    /// Add the given feature to the enabled ones.
    /// </summary>
    /// <param name="configFeature">Feature to add.</param>
    public void AddFeature(ConfigFeature configFeature)
    {
        _enabledFeatures.Add(configFeature.Name);
    }
    
    /// <summary>
    /// Add the given feature to the enabled ones.
    /// </summary>
    /// <param name="feature">Feature to add.</param>
    public void AddFeature(string feature)
    {
        _enabledFeatures.Add(feature);
    }
}
