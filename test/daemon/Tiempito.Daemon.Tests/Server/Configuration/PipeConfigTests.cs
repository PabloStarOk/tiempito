using System.Text;

using Tiempito.Daemon.Server.Configuration;

namespace Tiempito.Daemon.Tests.Server.Configuration;

/// <summary>
/// Unit tests for <see cref="PipeConfig"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "Server")]
public sealed class PipeConfigTests
{
    /// <summary>
    /// Returns test data mapping encoding name strings to their expected <see cref="Type"/> implementations.
    /// </summary>
    /// <returns>A <see cref="TheoryData{T1, T2}"/> where T1 is the encoding name and T2 is the expected <see cref="Type"/>.</returns>
    public static TheoryData<string, Type> PipeEncodingData()
    {
        var theoryData = new TheoryData<string, Type>
        {
            { "utf-8", typeof(UTF8Encoding) },
            { "utf-16", typeof(UnicodeEncoding) },
            { "utf-32", typeof(UTF32Encoding) },
            { "ascii", typeof(ASCIIEncoding) },
        };
        return theoryData;
    }

    /// <summary>
    /// Verifies that <see cref="PipeConfig.GetEncoding"/> returns an <see cref="Encoding"/>
    /// instance corresponding to the provided encoding name.
    /// </summary>
    /// <param name="encodingString">The encoding name to resolve (e.g. "utf-8").</param>
    /// <param name="expectedEncoding">The expected <see cref="Type"/> of the resulting encoding.</param>
    [Theory]
    [MemberData(nameof(PipeEncodingData))]
    public void GetEncoding_should_ReturnExpectedEncoding(string encodingString, Type expectedEncoding)
    {
        // Arrange
        var config = new PipeConfig
        {
            PipeEncoding = encodingString,
        };

        // Act
        Encoding actual = config.GetEncoding();

        // Assert
        Assert.IsType(expectedEncoding, actual, exactMatch: false);
    }
}