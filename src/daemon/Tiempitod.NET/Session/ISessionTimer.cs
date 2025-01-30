using AsyncEvent;

namespace Tiempitod.NET.Session;

/// <summary>
/// Defines a runner of sessions.
/// </summary>
public interface ISessionTimer
{
    /// <summary>
    /// Raised when a focus or break time is completed in a session.
    /// </summary>
    public event AsyncEventHandler<TimeType> OnTimeCompleted;
    
    /// <summary>
    /// Raised when a session is started.
    /// </summary>
    public event AsyncEventHandler OnSessionStarted;
    
    /// <summary>
    /// Raised when a session is completed.
    /// </summary>
    public event AsyncEventHandler<Session> OnSessionCompleted;
    
    /// <summary>
    /// Raised when a second has elapsed in a delay between times.
    /// </summary>
    public event AsyncEventHandler<TimeSpan> OnDelayElapsed;
    
    // TODO: Make async methods.
    /// <summary>
    /// Starts a timer for a session.
    /// </summary>
    /// <param name="session">Session to use to start the timer.</param>
    /// <param name="cancellationToken">Token to cancel the created timer.</param>
    public void Start(Session session, CancellationToken cancellationToken);
    
    /// <summary>
    /// Stops a timer for the given session ID.
    /// </summary>
    /// <param name="sessionId">ID of the session to stop its timer.</param>
    /// <returns>The modified session with the current cycle and states.</returns>
    public Session Stop(string sessionId);

    /// <summary>
    /// Stops all timers.
    /// </summary>
    /// <returns></returns>
    public Session[] StopAll();
}
