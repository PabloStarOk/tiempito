namespace Tiempito.Daemon.Domain.Config;

/// <summary>
/// Represents the configuration for a session.
/// </summary>
/// <param name="Id">Unique identifier for the configuration.</param>
/// <param name="TargetCycles">Number of cycles to target in the session.</param>
/// <param name="DelayBetweenTimes">Delay between each cycle.</param>
/// <param name="FocusDuration">Duration of the focus period.</param>
/// <param name="BreakDuration">Duration of the break period.</param>
public sealed record SessionConfig(
    string Id,
    int TargetCycles,
    TimeSpan DelayBetweenTimes,
    TimeSpan FocusDuration,
    TimeSpan BreakDuration)
{
    /// <summary>
    /// Gets the normalized identifier for the configuration.
    /// </summary>
    /// <remarks>
    /// Normalization trims leading/trailing whitespace and converts the identifier
    /// to lower-case using the invariant culture.
    /// </remarks>
    public string NormalizedId => NormalizeId(Id);

    /// <summary>
    /// Trims leading and trailing whitespace from <paramref name="id"/> and converts it to lower-case using the invariant culture.
    /// </summary>
    /// <param name="id">Identifier to normalize. Must not be <c>null</c>.</param>
    /// <returns>The normalized identifier.</returns>
    public static string NormalizeId(string id) => id.Trim().ToLowerInvariant();
}
