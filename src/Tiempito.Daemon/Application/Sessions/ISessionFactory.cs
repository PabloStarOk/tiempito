using Tiempito.Daemon.Domain.Sessions;

namespace Tiempito.Daemon.Application.Sessions;

/// <summary>
/// Factory interface for creating <see cref="ISession"/> instances and checking configuration existence.
/// </summary>
public interface ISessionFactory
{
    /// <summary>
    /// Creates a new <see cref="ISession"/> instance.
    /// </summary>
    /// <param name="id">The unique identifier for the session.</param>
    /// <param name="configId">The configuration identifier associated with the session.</param>
    /// <returns>A new <see cref="ISession"/> instance.</returns>
    public ISession Create(string id, string configId);

    /// <summary>
    /// Checks if a configuration with the specified ID exists.
    /// </summary>
    /// <param name="configId">The configuration identifier to check.</param>
    /// <returns><c>true</c> if the configuration exists; otherwise, <c>false</c>.</returns>
    public bool ExistsConfig(string configId);
}