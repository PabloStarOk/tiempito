using Tiempitod.NET.Sessions.Enums;
using Tiempitod.NET.Sessions.Interfaces;
using Tiempitod.NET.Sessions.Objects;

namespace Tiempitod.NET.Sessions;

/// <summary>
/// A storage of sessions.
/// </summary>
public class SessionStorage : ISessionStorage
{
    private readonly ILogger<SessionStorage> _logger;
    
    private readonly Dictionary<string, Session> _runningSessions = [];
    private readonly Dictionary<string, Session> _pausedSessions = [];
    private readonly Dictionary<string, Session> _cancelledSessions = [];
    private readonly Dictionary<string, Session> _finishedSessions = [];
    
    public IReadOnlyDictionary<string, Session> RunningSessions => _runningSessions.AsReadOnly();
    public IReadOnlyDictionary<string, Session> PausedSessions => _pausedSessions.AsReadOnly();
    public IReadOnlyDictionary<string, Session> CancelledSessions => _cancelledSessions.AsReadOnly();
    public IReadOnlyDictionary<string, Session> FinishedSessions => _finishedSessions.AsReadOnly();

    public SessionStorage(ILogger<SessionStorage> logger)
    {
        _logger = logger;
    }
    
    public bool AddSession(SessionStatus status, Session session)
    {
        Dictionary<string, Session> targetDictionary = GetTargetDictionary(status);
        session.Status = status;
        return targetDictionary.TryAdd(session.Id, session);
    }

    public void UpdateSession(SessionStatus status, Session session)
    {
        Dictionary<string, Session> targetDictionary = GetTargetDictionary(status);
        if (!targetDictionary.ContainsKey(session.Id))
        {
            _logger.LogCritical("Session with ID '{SessionId}' couldn't be updated because it doesn't exist in the dictionary.", session.Id);
            return;
        }
        targetDictionary[session.Id] = session;
    }

    public Session RemoveSession(SessionStatus status, string sessionId)
    {
        IDictionary<string, Session> targetDictionary = GetTargetDictionary(status);
        targetDictionary.Remove(sessionId, out Session removedSession);
        return removedSession;
    }

    /// <summary>
    /// Gets the target dictionary according to the <see cref="SessionStatus"/>.
    /// </summary>
    /// <param name="status">A <see cref="SessionStatus"/>.</param>
    /// <returns>A dictionary which represents the sessions with the given <see cref="SessionStatus"/>.</returns>
    /// <exception cref="NotImplementedException">If the <see cref="SessionStatus"/> is <see cref="SessionStatus.None"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If the given <see cref="SessionStatus"/> is undefined.</exception>
    private Dictionary<string, Session> GetTargetDictionary(SessionStatus status)
    {
        return status switch
        {
            SessionStatus.Executing => _runningSessions,
            SessionStatus.Paused => _pausedSessions,
            SessionStatus.Cancelled => _cancelledSessions,
            SessionStatus.Finished => _finishedSessions,
            SessionStatus.None => throw new NotImplementedException("Dictionary for none session status doesn't exist."),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}
