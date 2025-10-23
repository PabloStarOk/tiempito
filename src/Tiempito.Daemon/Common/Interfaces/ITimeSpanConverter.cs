using Tiempito.Daemon.Common.Enums;

namespace Tiempito.Daemon.Common.Interfaces;

/// <summary>
/// Defines a converter of <see cref="string"/> and <see cref="TimeSpan"/>
/// to represent time in string and TimeSpan formats.
/// </summary>
public interface ITimeSpanConverter
{
    /// <summary>
    /// Tries to convert a string into a <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="value">A <see cref="string"/> value to convert.</param>
    /// <param name="result">Converted result into a <see cref="TimeSpan"/>.</param>
    /// <returns>True if the value was converted successfully, false otherwise.</returns>
    public bool TryConvert(string value, out TimeSpan result);

    /// <summary>
    /// Converts a <see cref="TimeSpan"/> to a string representation
    /// using the highest possible time unit (hour, minute, second).
    /// </summary>
    /// <param name="value">A <see cref="TimeSpan"/> to convert to string.</param>
    /// <returns>A <see cref="string"/>.</returns>
    public string ConvertToString(TimeSpan value);
}