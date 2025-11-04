using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Sessions.ValueObjects;

namespace Tiempito.Daemon.Domain.Sessions;

/// <summary>
/// Represents a session with asynchronous disposal capabilities.
/// </summary>
public interface ISession : IAsyncDisposable
{
    /// <summary>
    /// Gets the unique identifier for the session.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the configuration settings for the session.
    /// </summary>
    public SessionConfig Configuration { get; }

    /// <summary>
    /// Gets the current state of the session.
    /// </summary>
    public SessionState State { get; }

    /// <summary>
    /// Gets or sets event triggered when a second has elapsed in the session.
    /// </summary>
    public Func<ISession, ValueTask>? SecondElapsedAsync { get; set; }

    /// <summary>
    /// Gets or sets event triggered when the configured interval is completed in the session.
    /// </summary>
    public Func<ISession, ValueTask>? IntervalCompletedAsync { get; set; }

    /// <summary>
    /// Gets or sets event triggered when the session is completed.
    /// </summary>
    public Func<ISession, ValueTask>? CompletedAsync { get; set; }

    /// <summary>
    /// Starts the session.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to stop the session.</param>
    public void Start(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels the session and releases resources.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> that completes when cancellation and cleanup are finished.</returns>
    public ValueTask CancelAsync();

    /// <summary>
    /// Pauses the session.
    /// </summary>
    public void Pause();

    /// <summary>
    /// Resumes the session.
    /// </summary>
    public void Resume();
}