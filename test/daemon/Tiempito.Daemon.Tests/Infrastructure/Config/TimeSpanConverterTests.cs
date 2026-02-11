using Tiempito.Daemon.Infrastructure.Config;

namespace Tiempito.Daemon.Tests.Infrastructure.Config;

/// <summary>
/// Unit tests for the <see cref="TimeSpanConverter"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "Config")]
public sealed class TimeSpanConverterTests
{
    private readonly TimeSpanConverter _converter;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSpanConverterTests"/> class.
    /// </summary>
    public TimeSpanConverterTests()
    {
        _converter = new TimeSpanConverter();
    }

    /// <summary>
    /// Provides test cases for <see cref="TimeSpanConverter.TryConvert"/>.
    /// Each test case consists of an input string and the expected <see cref="TimeSpan"/> result.
    /// </summary>
    /// <returns>A <see cref="TheoryData{T1, T2}"/> containing input strings and their expected <see cref="TimeSpan"/> results.</returns>
    public static TheoryData<string, TimeSpan> TryConvertTestCases()
    {
        return new TheoryData<string, TimeSpan>
        {
            { "5s", TimeSpan.FromSeconds(5) },
            { "24.5m", TimeSpan.FromMinutes(24.5) },
            { "1h", TimeSpan.FromHours(1) },
            { "3ms", TimeSpan.FromMilliseconds(3) },
            { "2.5d", TimeSpan.FromDays(2.5) },
        };
    }

    /// <summary>
    /// Provides test cases for <see cref="TimeSpanConverter.Format"/>.
    /// Each test case consists of a <see cref="TimeSpan"/> and its expected string representation.
    /// </summary>
    /// <returns>A <see cref="TheoryData{T1, T2}"/> containing <see cref="TimeSpan"/> values and their expected string representations.</returns>
    public static TheoryData<TimeSpan, string> FormatTestCases()
    {
        return new TheoryData<TimeSpan, string>
        {
            { TimeSpan.FromDays(1.5), "1.5d" },
            { TimeSpan.FromMilliseconds(110), "110ms" },
            { TimeSpan.FromHours(2), "2h" },
            { TimeSpan.FromMinutes(12.25), "12.25m" },
            { TimeSpan.FromSeconds(10), "10s" },
        };
    }

    /// <summary>
    /// Tests that <see cref="TimeSpanConverter.TryConvert"/> returns true and converts the given string to the expected <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="input">The input string representing a time span.</param>
    /// <param name="expected">The expected <see cref="TimeSpan"/> result.</param>
    [Theory]
    [MemberData(nameof(TryConvertTestCases))]
    public void TryConvert_should_ReturnTrueAndConvertGivenStringToExpectedTimeSpan(string input, TimeSpan expected)
    {
        // Act
        bool actual = _converter.TryConvert(input, out TimeSpan actualTimeSpan);

        // Assert
        Assert.True(actual);
        Assert.Equal(expected, actualTimeSpan);
    }

    /// <summary>
    /// Tests that <see cref="TimeSpanConverter.TryConvert"/> returns false and outputs <see cref="TimeSpan.Zero"/> when given an invalid string.
    /// </summary>
    [Fact]
    public void TryConvert_should_ReturnFalse_when_GivenStringIsInvalid()
    {
        // Arrange
        const string input = "invalid";

        // Act
        bool actual = _converter.TryConvert(input, out TimeSpan actualTimeSpan);

        // Assert
        Assert.False(actual);
        Assert.Equal(TimeSpan.Zero, actualTimeSpan);
    }

    /// <summary>
    /// Tests that <see cref="TimeSpanConverter.Format"/> formats the given <see cref="TimeSpan"/> to the expected string representation.
    /// </summary>
    /// <param name="input">The <see cref="TimeSpan"/> to format.</param>
    /// <param name="expected">The expected string representation of the <see cref="TimeSpan"/>.</param>
    [Theory]
    [MemberData(nameof(FormatTestCases))]
    public void Format_should_FormatGivenTimeSpanToExpectedString(TimeSpan input, string expected)
    {
        // Act
        string actual = _converter.Format(input);

        // Assert
        Assert.Equal(expected, actual);
    }
}