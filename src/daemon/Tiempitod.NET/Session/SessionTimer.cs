using Timer = System.Threading.Timer;

namespace Tiempitod.NET.Session;

/// <summary>
/// A manager of timers of sessions.
/// </summary>
public class SessionTimer : ISessionTimer
{
    private readonly IProgress<Session> _timeProgress;
    private readonly ISessionStorage _sessionStorage;
    private readonly Dictionary<string, Timer> _timers = [];
    private readonly TimeSpan _interval;
    
    public event EventHandler<TimeType>? OnTimeCompleted;
    public event EventHandler? OnSessionStarted;
    public event EventHandler<Session>? OnSessionCompleted;
    
    /// <summary>
    /// Instantiates a <see cref="SessionTimer"/>.
    /// </summary>
    /// <param name="timeProgress">Reporter of progress.</param>
    /// <param name="sessionStorage">Storage of sessions.</param>
    public SessionTimer(IProgress<Session> timeProgress, ISessionStorage sessionStorage)
    {
        _timeProgress = timeProgress;
        _sessionStorage = sessionStorage;
        _interval = TimeSpan.FromSeconds(1);
    }
    
    public void Start(Session session, CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Stop(session.Id));
        
        if (!_sessionStorage.AddSession(SessionStatus.Executing, session))
            return;
        
        var timer = new Timer(_ => TimerCallback(session.Id), null, 1000, 1000);
        if (session.Status is not SessionStatus.Paused)
            OnSessionStarted?.Invoke(this, EventArgs.Empty);
        _timers.Add(session.Id, timer);
    }

    public Session Stop(string sessionId)
    {
        _timers.Remove(sessionId, out Timer? timer);
        timer?.Dispose();
        return _sessionStorage.RemoveSession(SessionStatus.Executing, sessionId);
    }

    /// <summary>
    /// Updates the elapsed time of a session and invokes the
    /// <see cref="OnTimeCompleted"/> event.
    /// </summary>
    /// <param name="sessionId">ID of the session the callback will be managing.</param>
    private void TimerCallback(string sessionId)
    {
        Session session = _sessionStorage.RunningSessions[sessionId];
        session.Elapsed += _interval;
        _sessionStorage.UpdateSession(SessionStatus.Executing, session);
        _timeProgress.Report(session);

        if (session.Elapsed < GetTargetDuration(session))
            return;

        OnTimeCompleted?.Invoke(this, session.CurrentTimeType);
        session = SwitchTime(session);

        if (session.CurrentCycle >= session.TargetCycles)
        {
            CompleteSession(session.Id);
            return;
        }

        _sessionStorage.UpdateSession(SessionStatus.Executing, session);
    }

    /// <summary>
    /// Completes a session which has reached its target cycles and invokes
    /// the <see cref="OnSessionCompleted"/> event.
    /// </summary>
    /// <param name="sessionId">ID of the session to complete.</param>
    private void CompleteSession(string sessionId)
    {
        _timers.Remove(sessionId, out Timer? timer);
        timer?.Dispose();

        Session finishedSession = _sessionStorage.RemoveSession(SessionStatus.Executing, sessionId);
        OnSessionCompleted?.Invoke(this, finishedSession);
    }

    /// <summary>
    /// Switches the time type (focus or break) of the given session.
    /// </summary>
    /// <param name="session">Session to switch its time type.</param>
    /// <returns>The new session with the switched time type.</returns>
    private static Session SwitchTime(Session session)
    {
        if (session.CurrentTimeType is TimeType.Focus)
            session.CurrentTimeType = TimeType.Break;
        else
        {
            session.CurrentCycle++;
            session.CurrentTimeType = TimeType.Focus;
        }
        session.Elapsed = TimeSpan.Zero;
        return session;
    }
    
    /// <summary>
    /// Gets the target duration of the session according to
    /// its current time type.
    /// </summary>
    /// <param name="session">Session to use.</param>
    /// <returns>A <see cref="TimeSpan"/> with the duration.</returns>
    private static TimeSpan GetTargetDuration(Session session)
    {
        return session.CurrentTimeType is TimeType.Focus 
            ? session.FocusDuration 
            : session.BreakDuration;
    }
}
