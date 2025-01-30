using AsyncEvent;

namespace Tiempitod.NET.Session;

/// <summary>
/// A manager of timers of sessions.
/// </summary>
public class SessionTimer : ISessionTimer
{
    private readonly TimeProvider _timeProvider;
    private readonly IProgress<Session> _timeProgress;
    private readonly ISessionStorage _sessionStorage;
    private readonly Dictionary<string, ITimer> _timers = [];
    private readonly Dictionary<string, TimeSpan> _sessionsDelays = [];
    private readonly TimeSpan _interval;
    
    public event AsyncEventHandler<TimeType>? OnTimeCompleted;
    public event AsyncEventHandler? OnSessionStarted;
    public event AsyncEventHandler<Session>? OnSessionCompleted;
    public event AsyncEventHandler<TimeSpan>? OnDelayElapsed;
    
    /// <summary>
    /// Instantiates a <see cref="SessionTimer"/>.
    /// </summary>
    /// <param name="timeProvider">Provider of time to create timers.</param>
    /// <param name="timeProgress">Reporter of progress.</param>
    /// <param name="sessionStorage">Storage of sessions.</param>
    /// <param name="interval">Interval of time to use for the timers.</param>
    public SessionTimer(
        TimeProvider timeProvider, IProgress<Session> timeProgress,
        ISessionStorage sessionStorage, [FromKeyedServices("TimingInterval")] TimeSpan interval)
    {
        _timeProvider = timeProvider;
        _timeProgress = timeProgress;
        _sessionStorage = sessionStorage;
        _interval = interval;
    }
    
    public void Start(Session session, CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Stop(session.Id));
        
        if (!_sessionStorage.AddSession(SessionStatus.Executing, session))
            return;
        
        ITimer timer = _timeProvider.CreateTimer(_ => TimerCallback(session.Id), null, _interval, _interval);
        if (session.Status is not SessionStatus.Paused)
            OnSessionStarted?.Invoke(this, EventArgs.Empty);
        _timers.Add(session.Id, timer);
    }

    public Session Stop(string sessionId)
    {
        _timers.Remove(sessionId, out ITimer? timer);
        _sessionsDelays.Remove(sessionId);
        timer?.Dispose();
        
        return _sessionStorage.RemoveSession(SessionStatus.Executing, sessionId);
    }

    public Session[] StopAll()
    {
        List<Session> tempSessions = [];
        tempSessions.AddRange(_timers.Keys.Select(Stop));
        return tempSessions.ToArray();
    }
    
    /// <summary>
    /// Updates the elapsed time of a session and invokes the
    /// <see cref="OnTimeCompleted"/> event.
    /// </summary>
    /// <param name="sessionId">ID of the session the callback will be managing.</param>
    private void TimerCallback(string sessionId)
    {
        Session session = _sessionStorage.RunningSessions[sessionId];
        
        if (IsOnDelay(session))
            return;
        
        // Add elapsed second.
        session.Elapsed += _interval;
        _sessionStorage.UpdateSession(SessionStatus.Executing, session);
        _timeProgress.Report(session);

        if (session.Elapsed < GetTargetDuration(session))
            return;

        // Time completed
        OnTimeCompleted?.Invoke(this, session.CurrentTimeType);
        session = SwitchTime(session);
        _sessionStorage.UpdateSession(SessionStatus.Executing, session);
        
        // Session completed
        if (session.TargetCycles > 0 // 0 means infinite cycles. 
            && session.CurrentCycle >= session.TargetCycles)
            CompleteSession(session.Id);
        
        // Add delay if exists.
        if (session.DelayBetweenTimes > TimeSpan.Zero)
            _sessionsDelays.Add(sessionId, TimeSpan.Zero);
    }

    /// <summary>
    /// Completes a session which has reached its target cycles and invokes
    /// the <see cref="OnSessionCompleted"/> event.
    /// </summary>
    /// <param name="sessionId">ID of the session to complete.</param>
    private void CompleteSession(string sessionId)
    {
        _timers.Remove(sessionId, out ITimer? timer);
        _sessionsDelays.Remove(sessionId);
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

    /// <summary>
    /// Checks if a session is on a delay between times.
    /// </summary>
    /// <param name="session">Session to check.</param>
    /// <returns>True if it's on a delay, false otherwise.</returns>
    private bool IsOnDelay(Session session)
    {
        if (!_sessionsDelays.TryGetValue(session.Id, out TimeSpan delay))
            return false;

        if (delay >= session.DelayBetweenTimes)
        {
            _sessionsDelays.Remove(session.Id);
            return false;
        }
        
        delay += _interval;
        OnDelayElapsed?.Invoke(this, delay);
        _sessionsDelays[session.Id] = delay;
        return true;
    }
}
