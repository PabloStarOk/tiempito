namespace Tiempito.Daemon.Application.Config;

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
    /// Converts a <see cref="TimeSpan"/> value to its string representation.
    /// </summary>
    /// <param name="value">The <see cref="TimeSpan"/> to format.</param>
    /// <returns>A string representation of the <see cref="TimeSpan"/>.</returns>
    public string Format(TimeSpan value);
}