using Tiempito.Daemon.Sessions.Enums;
using Tiempito.Daemon.Sessions.Objects;

namespace Tiempito.Daemon.Sessions.Interfaces;

/// <summary>
/// Defines a storage of sessions which stores all managed sessions in dictionaries identified by
/// the <see cref="SessionStatus"/> enum.
/// </summary>
public interface ISessionStorage
{
    /// <summary>
    /// All sessions that are running.
    /// </summary>
    public IReadOnlyDictionary<string, Session> RunningSessions { get; }
    
    /// <summary>
    /// All currently paused sessions.
    /// </summary>
    public IReadOnlyDictionary<string, Session> PausedSessions { get; }
    
    /// <summary>
    /// All cancelled sessions.
    /// </summary>
    public IReadOnlyDictionary<string, Session> CancelledSessions { get; }
    
    /// <summary>
    /// All finished sessions.
    /// </summary>
    public IReadOnlyDictionary<string, Session> FinishedSessions { get; }

    /// <summary>
    /// Adds a session to the dictionary according to the status.
    /// </summary>
    /// <param name="status">A <see cref="SessionStatus"/> to know the dictionary where the session must be stored.</param>
    /// <param name="session">Session to store.</param>
    /// <returns>True if the session was added successfully, false otherwise.</returns>
    public bool AddSession(SessionStatus status, Session session);
    
    /// <summary>
    /// Updates the session with the given session.
    /// </summary>
    /// <param name="status">A <see cref="SessionStatus"/> to know the dictionary where the session is stored.</param>
    /// <param name="session">Session to update with.</param>
    public void UpdateSession(SessionStatus status, Session session);
    
    /// <summary>
    /// Removes a session from a dictionary.
    /// </summary>
    /// <param name="status">A <see cref="SessionStatus"/> to know the dictionary where the session must be removed from.</param>
    /// <param name="sessionId">ID of the session to remove.</param>
    /// <returns>The removed session.</returns>
    public Session RemoveSession(SessionStatus status, string sessionId);
}
