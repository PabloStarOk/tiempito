using Tiempito.Daemon.Common.Enums;
using Tiempito.Daemon.Common.Interfaces;

namespace Tiempito.Daemon.Common.Services;

/// <summary>
/// Converts a <see cref="string"/> to <see cref="TimeSpan"/>
/// and vice versa.
/// </summary>
public class TimeSpanConverter : ITimeSpanConverter
{
    /// <summary>
    /// Maps enum <see cref="TimeUnit"/> to a string representing that unit in lower case.
    /// </summary>
    private readonly Dictionary<TimeUnit, string> _timeUnitsSymbols = new()
    {
        { TimeUnit.Millisecond, "ms" },
        { TimeUnit.Second, "s" },
        { TimeUnit.Minute, "m" },
        { TimeUnit.Hour, "h" },
        { TimeUnit.Day, "d" }
    };
    
    /// <inheritdoc/>
    public bool TryConvert(string value, out TimeSpan result)
    {
        result = TimeSpan.Zero;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (value is "0")
            return true;

        if (!TryGetTimeUnit(value, out TimeUnit timeUnit))
            return false;
        
        string amountStr = value.Replace(_timeUnitsSymbols[timeUnit], "");
        
        if (!double.TryParse(amountStr, out double timeSpanAmount))
            return false;

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
    public string ConvertToString(TimeSpan value)
    {
        if (value.Days > 0)
            return $"{value.TotalDays:0.##}{_timeUnitsSymbols[TimeUnit.Day]}";
        if (value.Hours > 0)
            return $"{value.TotalHours:0.##}{_timeUnitsSymbols[TimeUnit.Hour]}";
        if (value.Minutes > 0)
            return $"{value.TotalMinutes:0.##}{_timeUnitsSymbols[TimeUnit.Minute]}";
        if (value.Seconds > 0)
            return $"{value.TotalSeconds:0.##}{_timeUnitsSymbols[TimeUnit.Second]}";
        
        return $"{value.TotalMilliseconds:0.##}{_timeUnitsSymbols[TimeUnit.Millisecond]}";
    }
    
    /// <summary>
    /// Tries to determine the <see cref="TimeUnit"/> from the given string value.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="timeUnit">When this method returns, contains the <see cref="TimeUnit"/> value equivalent to the string, if the conversion succeeded, or zero if the conversion failed.</param>
    /// <returns><c>true</c> if the string was successfully parsed; otherwise, <c>false</c>.</returns>
    private bool TryGetTimeUnit(string value, out TimeUnit timeUnit)
    {
        timeUnit = 0;
        TimeUnit[] timeUnits = Enum.GetValues<TimeUnit>();
        
        foreach (TimeUnit unit in timeUnits)
        {
            if (!value.EndsWith(_timeUnitsSymbols[unit]))
                continue;
    
            timeUnit = unit;
            return true;
        }
    
        return false;
    }
}