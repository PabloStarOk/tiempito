using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Shared;
using Tiempito.IPC.Models.Enums;

namespace Tiempito.Daemon.Application.Config.User;

/// <summary>
/// Defines a services to manage user's configuration.
/// </summary>
public interface IUserConfigService
{
    /// <summary>
    /// Gets user's configuration.
    /// </summary>
    public UserConfig UserConfig { get; }

    /// <summary>
    /// Executed when user's configuration is changed.
    /// </summary>
    public event EventHandler? OnConfigChanged;

    /// <summary>
    /// Changes the default session configuration to use when
    /// session configuration IDs are not provided.
    /// </summary>
    /// <param name="id">ID of the new session configuration or null if none.</param>
    /// <returns>An <see cref="OperationResult"/>.</returns>
    public Task<OperationResult> ChangeDefaultSessionConfigAsync(string? id);

    /// <summary>
    /// Enables a feature to use for the user.
    /// </summary>
    /// <param name="feature">Feature to enable.</param>
    /// <returns>An <see cref="OperationResult"/>.</returns>
    public Task<OperationResult> EnableFeatureAsync(UserFeature feature);

    /// <summary>
    /// Disables a feature for the user.
    /// </summary>
    /// <param name="feature">Feature to disable.</param>
    /// <returns>An <see cref="OperationResult"/>.</returns>
    public Task<OperationResult> DisableFeatureAsync(UserFeature feature);
}