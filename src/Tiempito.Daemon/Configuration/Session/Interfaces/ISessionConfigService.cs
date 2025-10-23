using Tiempito.Daemon.Common;
using Tiempito.Daemon.Configuration.Session.Objects;

namespace Tiempito.Daemon.Configuration.Session.Interfaces;

/// <summary>
/// Defines a service to manage sessions configuration.
/// </summary>
public interface ISessionConfigService
{
    /// <summary>
    /// Default session configuration of the user.
    /// </summary>
    public SessionConfig DefaultConfig { get; }
    
    /// <summary>
    /// All session configurations of the user.
    /// </summary>
    public IReadOnlyDictionary<string, SessionConfig> Configs { get; }

    /// <summary>
    /// Tries to get a session configuration by its ID.
    /// </summary>
    /// <returns>A <see cref="SessionConfig"/>.</returns>
    public bool TryGetConfigById(string id, out SessionConfig config);
    
    /// <summary>
    /// Adds a new session configuration. 
    /// </summary>
    /// <param name="config">Session configuration to add.</param>
    /// <returns>An <see cref="OperationResult"/>.</returns>
    public Task<OperationResult> AddConfigAsync(SessionConfig config);
    
    /// <summary>
    /// Modifies an existing session configuration
    /// (Represents a PUT or a PATCH)
    /// </summary>
    /// <param name="configId">ID of the session configuration to modify.</param>
    /// <param name="targetCycles">New target cycles</param>
    /// <param name="delayBetweenTimes">New delay between times.</param>
    /// <param name="focusDuration">New focus duration.</param>
    /// <param name="breakDuration">New break duration.</param>
    /// <returns>An <see cref="OperationResult"/>.</returns>
    public Task<OperationResult> ModifyConfigAsync(
        string configId,
        int? targetCycles = null,
        TimeSpan? delayBetweenTimes = null,
        TimeSpan? focusDuration = null,
        TimeSpan? breakDuration = null);
    
    // TODO: Add RemoveConfigAsync method.
}