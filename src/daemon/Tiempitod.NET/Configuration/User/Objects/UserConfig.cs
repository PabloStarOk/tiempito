using System.Collections.Immutable;

namespace Tiempitod.NET.Configuration.User.Objects;

/// <summary>
/// Represents the configuration of the user.
/// </summary>
public struct UserConfig
{
    private readonly List<string> _enabledFeatures = [];
    
    /// <summary>
    /// The id of the default session to start by the daemon.
    /// </summary>
    public string DefaultSessionId { get; private set; }

    /// <summary>
    /// If the notifications feature is enabled.
    /// </summary>
    public bool NotificationsEnabled => _enabledFeatures.Contains("notification");
    
    /// <summary>
    /// All enabled features.
    /// </summary>
    public IReadOnlyList<string> EnabledFeatures => _enabledFeatures;
    
    /// <summary>
    /// Allowed features that can be understood by the daemon.
    /// </summary>
    public static ImmutableArray<UserConfigFeature> AllowedFeatures { get; }  =
    [
        ..new UserConfigFeature[]
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
    /// Sets the default session configuration ID.
    /// </summary>
    /// <param name="id">ID of the session configuration.</param>
    public void SetDefaultSessionConfigId(string id)
    {
        DefaultSessionId = id;
    }
    
    /// <summary>
    /// Add the given feature to the enabled ones.
    /// </summary>
    /// <param name="configFeature">Feature to add.</param>
    public void AddFeature(UserConfigFeature configFeature)
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
    
    /// <summary>
    /// Remove a feature from the enabled ones.
    /// </summary>
    /// <param name="configFeature">Feature to remove.</param>
    public void RemoveFeature(UserConfigFeature configFeature)
    {
        _enabledFeatures.Remove(configFeature.Name);
    }
}
