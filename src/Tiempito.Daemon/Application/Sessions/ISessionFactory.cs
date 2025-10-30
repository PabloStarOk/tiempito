using Tiempito.Daemon.Domain.Sessions;

namespace Tiempito.Daemon.Application.Sessions;

/// <summary>
/// Factory interface for creating <see cref="Session"/> instances and checking configuration existence.
/// </summary>
public interface ISessionFactory
{
    /// <summary>
    /// Creates a new <see cref="Session"/> instance.
    /// </summary>
    /// <param name="id">The unique identifier for the session.</param>
    /// <param name="configId">The configuration identifier associated with the session.</param>
    /// <param name="onSecondElapsedAsync">Callback invoked every second elapsed in the session.</param>
    /// <param name="onIntervalCompletedAsync">Callback invoked when an interval is completed.</param>
    /// <param name="onCompletedAsync">Callback invoked when the session is completed.</param>
    /// <returns>A new <see cref="Session"/> instance.</returns>
    public Session Create(
        string id,
        string configId,
        Func<Session, ValueTask> onSecondElapsedAsync,
        Func<Session, ValueTask> onIntervalCompletedAsync,
        Func<Session, ValueTask> onCompletedAsync);

    /// <summary>
    /// Checks if a configuration with the specified ID exists.
    /// </summary>
    /// <param name="configId">The configuration identifier to check.</param>
    /// <returns><c>true</c> if the configuration exists; otherwise, <c>false</c>.</returns>
    public bool ExistsConfig(string configId);
}