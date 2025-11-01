using MessagePack;

namespace Tiempito.IPC.Models.Commands.Config;

/// <summary>
/// Represents a command to modify an existing session configuration.
/// </summary>
[MessagePackObject(AllowPrivate = true)]
public sealed record ModifySessionConfigCommand : Command
{
    /// <summary>
    /// Gets the unique identifier for the session configuration.
    /// </summary>
    [Key(3)]
    public string SessionConfigId { get; }

    /// <summary>
    /// Gets the target number of cycles for the session.
    /// </summary>
    [Key(4)]
    public uint? TargetCycles { get; }

    /// <summary>
    /// Gets the duration of the focus period.
    /// </summary>
    [Key(5)]
    public string? FocusDuration { get; }

    /// <summary>
    /// Gets the duration of the break period.
    /// </summary>
    [Key(6)]
    public string? BreakDuration { get; }

    /// <summary>
    /// Gets the delay between times.
    /// </summary>
    [Key(7)]
    public string? DelayBetweenTimes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModifySessionConfigCommand"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the command.</param>
    /// <param name="correlationId">The correlation identifier for tracking related messages.</param>
    /// <param name="timestamp">The timestamp when the command was created.</param>
    /// <param name="sessionConfigId">The unique identifier for the session configuration.</param>
    /// <param name="targetCycles">The target number of cycles for the session.</param>
    /// <param name="focusDuration">The duration of the focus period.</param>
    /// <param name="breakDuration">The duration of the break period.</param>
    /// <param name="delayBetweenTimes">The delay between periods.</param>
    internal ModifySessionConfigCommand(
        Guid id,
        Guid correlationId,
        DateTimeOffset timestamp,
        string sessionConfigId,
        uint? targetCycles,
        string? focusDuration,
        string? breakDuration,
        string? delayBetweenTimes)
        : base(id, correlationId, timestamp)
    {
        SessionConfigId = sessionConfigId;
        TargetCycles = targetCycles;
        FocusDuration = focusDuration;
        BreakDuration = breakDuration;
        DelayBetweenTimes = delayBetweenTimes;
    }

    /// <summary>
    /// Creates a new <see cref="ModifySessionConfigCommand"/> instance with the specified parameters.
    /// </summary>
    /// <param name="sessionConfigId">The unique identifier of the session configuration to modify.</param>
    /// <param name="targetCycles">The target number of cycles for the session.</param>
    /// <param name="focusDuration">The duration of the focus period, as a string.</param>
    /// <param name="breakDuration">The duration of the break period, as a string.</param>
    /// <param name="delayBetweenTimes">The delay between times, as a string.</param>
    /// <returns>A new <see cref="ModifySessionConfigCommand"/> object.</returns>
    public static ModifySessionConfigCommand CreateNew(
        string sessionConfigId,
        uint? targetCycles,
        string? focusDuration,
        string? breakDuration,
        string? delayBetweenTimes)
    {
        ArgumentNullException.ThrowIfNull(sessionConfigId);

        return new ModifySessionConfigCommand(
            id: Guid.NewGuid(),
            correlationId: Guid.NewGuid(),
            timestamp: DateTimeOffset.UtcNow,
            sessionConfigId,
            targetCycles,
            focusDuration,
            breakDuration,
            delayBetweenTimes);
    }
}