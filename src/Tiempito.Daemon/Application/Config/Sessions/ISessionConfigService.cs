using System.Diagnostics.CodeAnalysis;

using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Shared;

namespace Tiempito.Daemon.Application.Config.Sessions;

/// <summary>
/// Defines a service to manage sessions configuration.
/// </summary>
public interface ISessionConfigService
{
    /// <summary>
    /// Gets the default session configuration of the user.
    /// </summary>
    public SessionConfig DefaultConfig { get; }

    /// <summary>
    /// Tries to get a session configuration by its ID.
    /// </summary>
    /// <param name="id">The ID of the session configuration to retrieve.</param>
    /// <param name="config">When this method returns, contains the session configuration associated with the specified ID, if found; otherwise, null.</param>
    /// <returns><c>true</c> if the configuration was found; otherwise, <c>false</c>.</returns>
    public bool TryGetConfigById(string id, [NotNullWhen(true)] out SessionConfig? config);

    /// <summary>
    /// Adds a new session configuration.
    /// </summary>
    /// <param name="config">Session configuration to add.</param>
    /// <returns>An <see cref="OperationResult"/>.</returns>
    public Task<OperationResult> AddConfigAsync(SessionConfig config);

    /// <summary>
    /// Modifies an existing session configuration (Represents a PUT or a PATCH).
    /// </summary>
    /// <param name="configId">ID of the session configuration to modify.</param>
    /// <param name="targetCycles">New target cycles.</param>
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