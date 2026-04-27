using System.Globalization;
using System.Text.RegularExpressions;

using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Domain.Config.Enums;

namespace Tiempito.Daemon.Infrastructure.Config;

/// <summary>
/// Converts a <see cref="string"/> to <see cref="TimeSpan"/> and vice versa.
/// </summary>
public partial class TimeSpanConverter : ITimeSpanConverter
{
    [GeneratedRegex(@"^(\d+(?:\.\d+)?)\s*([a-zA-Z]+)$", RegexOptions.IgnoreCase)]
    private static partial Regex TimeSpanRegex();

    /// <summary>
    /// Maps enum <see cref="TimeUnit"/> to a string representing that unit in lower case.
    /// </summary>
    private readonly Dictionary<TimeUnit, string> _symbolsMap = new ()
    {
        { TimeUnit.Millisecond, "ms" },
        { TimeUnit.Second, "s" },
        { TimeUnit.Minute, "m" },
        { TimeUnit.Hour, "h" },
        { TimeUnit.Day, "d" },
    };

    private readonly Dictionary<string, TimeUnit> _timeUnitsMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSpanConverter"/> class.
    /// </summary>
    public TimeSpanConverter()
    {
        _timeUnitsMap = _symbolsMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }

    /// <inheritdoc/>
    public bool TryConvert(string value, out TimeSpan result)
    {
        result = TimeSpan.Zero;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (value is "0")
        {
            return true;
        }

        Match match = TimeSpanRegex().Match(value);
        if (!match.Success)
        {
            return false;
        }

        ReadOnlySpan<char> amountString = match.Groups[1].ValueSpan;
        if (!double.TryParse(amountString, NumberStyles.Float, CultureInfo.InvariantCulture, out double timeSpanAmount))
        {
            return false;
        }

        string timeUnitString = match.Groups[2].Value;
        if (!_timeUnitsMap.TryGetValue(timeUnitString, out TimeUnit timeUnit))
        {
            return false;
        }

        result = timeUnit switch
        {
            TimeUnit.Millisecond => TimeSpan.FromMilliseconds(timeSpanAmount),
            TimeUnit.Second => TimeSpan.FromSeconds(timeSpanAmount),
            TimeUnit.Minute => TimeSpan.FromMinutes(timeSpanAmount),
            TimeUnit.Hour => TimeSpan.FromHours(timeSpanAmount),
            TimeUnit.Day => TimeSpan.FromDays(timeSpanAmount),
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };

        return true;
    }

    /// <inheritdoc />
    public string Format(TimeSpan value)
    {
        if (value.Days > 0)
        {
            return $"{value.TotalDays:0.##}{_symbolsMap[TimeUnit.Day]}";
        }

        if (value.Hours > 0)
        {
            return $"{value.TotalHours:0.##}{_symbolsMap[TimeUnit.Hour]}";
        }

        if (value.Minutes > 0)
        {
            return $"{value.TotalMinutes:0.##}{_symbolsMap[TimeUnit.Minute]}";
        }

        if (value.Seconds > 0)
        {
            return $"{value.TotalSeconds:0.##}{_symbolsMap[TimeUnit.Second]}";
        }

        return $"{value.TotalMilliseconds:0.##}{_symbolsMap[TimeUnit.Millisecond]}";
    }
}