using Tiempito.Daemon.Domain.Sessions.Enums;

namespace Tiempito.Daemon.Domain.Sessions.ValueObjects;

/// <summary>
/// Represents the immutable state of a session, including timing, status, cycle, and interval information.
/// </summary>
public sealed record SessionState
{
    /// <summary>
    /// Gets the target duration for the current session interval.
    /// </summary>
    public TimeSpan TargetDuration { get; private init; }

    /// <summary>
    /// Gets the elapsed time in the current session interval.
    /// </summary>
    public TimeSpan ElapsedTime { get; private init; }

    /// <summary>
    /// Gets the current status of the session.
    /// </summary>
    public SessionStatus Status { get; private init; }

    /// <summary>
    /// Gets the current cycle number of the session.
    /// </summary>
    public int Cycle { get; private init; }

    /// <summary>
    /// Gets the type of interval currently active in the session.
    /// </summary>
    public SessionIntervalType IntervalType { get; private init; }

    private int CompletedIntervals { get; init; }

    /// <summary>
    /// Creates the initial <see cref="SessionState"/> with the specified target duration.
    /// </summary>
    /// <param name="targetDuration">The target duration for the initial session interval.</param>
    /// <returns>A new <see cref="SessionState"/> instance.</returns>
    public static SessionState CreateInitial(TimeSpan targetDuration)
    {
        return new SessionState
        {
            TargetDuration = targetDuration,
            ElapsedTime = TimeSpan.Zero,
            Status = SessionStatus.None,
            Cycle = 0,
            IntervalType = SessionIntervalType.Focus,
        };
    }

    /// <summary>
    /// Returns a new <see cref="SessionState"/> instance with the <c>ElapsedTime</c> property
    /// incremented by one second.
    /// </summary>
    /// <returns>A new <see cref="SessionState"/> with <c>ElapsedTime</c> increased by one second.</returns>
    public SessionState WithElapsedSecond()
    {
        return this with
        {
            ElapsedTime = ElapsedTime + TimeSpan.FromSeconds(1),
        };
    }

    /// <summary>
    /// Returns a new <see cref="SessionState"/> instance with the specified <paramref name="status"/>.
    /// </summary>
    /// <param name="status">The new session status to set.</param>
    /// <returns>A new <see cref="SessionState"/> with the updated status.</returns>
    public SessionState WithStatus(SessionStatus status)
    {
        return this with
        {
            Status = status,
        };
    }

    /// <summary>
    /// Returns a new <see cref="SessionState"/> instance with a new interval.
    /// Updates the interval type and target duration, resets elapsed time, and manages cycle and completed intervals.
    /// Throws <see cref="InvalidOperationException"/> if the interval is changed before reaching the target duration.
    /// </summary>
    /// <param name="nextInterval">The type of the next interval.</param>
    /// <param name="nextTargetDuration">The target duration for the next interval.</param>
    /// <returns>A new <see cref="SessionState"/> with updated interval information.</returns>
    public SessionState WithNewInterval(SessionIntervalType nextInterval, TimeSpan nextTargetDuration)
    {
        if (ElapsedTime < TargetDuration)
        {
            throw new InvalidOperationException("Cannot change interval before reaching target duration.");
        }

        int completedIntervals = CompletedIntervals;
        if (IntervalType is not SessionIntervalType.Delay)
        {
            completedIntervals++;
        }

        int cycle = Cycle;
        if (completedIntervals > 1)
        {
            cycle++;
            completedIntervals = 0;
        }

        return this with
        {
            TargetDuration = nextTargetDuration,
            ElapsedTime = TimeSpan.Zero,
            IntervalType = nextInterval,
            Cycle = cycle,
            CompletedIntervals = completedIntervals
        };
    }
}