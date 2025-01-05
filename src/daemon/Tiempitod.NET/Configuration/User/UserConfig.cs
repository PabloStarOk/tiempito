using System.Collections.Immutable;

namespace Tiempitod.NET.Configuration.User;

/// <summary>
/// Represents the configuration of the user.
/// </summary>
public struct UserConfig
{
    private readonly List<string> _enabledFeatures = [];
    
    /// <summary>
    /// The id of the default session to start by the daemon.
    /// </summary>
    public string DefaultSessionId { get; init; }
    
    /// <summary>
    /// If the notifications feature is enabled.
    /// </summary>
    public bool NotificationsEnabled { get; private set; }
    
    /// <summary>
    /// All enabled features.
    /// </summary>
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
        OnFeatureModified(configFeature.Name, true);
    }
    
    /// <summary>
    /// Add the given feature to the enabled ones.
    /// </summary>
    /// <param name="feature">Feature to add.</param>
    public void AddFeature(string feature)
    {
        _enabledFeatures.Add(feature);
        OnFeatureModified(feature, wasAdded: true);
    }
    
    /// <summary>
    /// Remove a feature from the enabled ones.
    /// </summary>
    /// <param name="configFeature">Feature to remove.</param>
    public void RemoveFeature(ConfigFeature configFeature)
    {
        _enabledFeatures.Remove(configFeature.Name);
        OnFeatureModified(configFeature.Name, wasAdded: false);
    }

    /// <summary>
    /// Updates the variables of this struct according to the added/removed
    /// features.
    /// </summary>
    /// <param name="feature">The feature that was modified.</param>
    /// <param name="wasAdded">If the feature was added.</param>
    private void OnFeatureModified(string feature, bool wasAdded)
    {
        switch (feature)
        {
            case "notification": // TODO: Replace hardcoded name with enum.
                NotificationsEnabled = wasAdded;
                break;
            
            default:
                return;
        }
    }
}
