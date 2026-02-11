using Tiempito.Daemon.Domain.Config;

namespace Tiempito.Daemon.Tests.Domain.Config;

/// <summary>
/// Unit tests for the <see cref="SessionConfig"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "Config")]
public sealed class SessionConfigTests
{
    /// <summary>
    /// Tests that <see cref="SessionConfig.NormalizedId"/> returns a trimmed, lower-invariant version of the ID.
    /// </summary>
    [Fact]
    public void NormalizedId_should_ReturnATrimmedLowerInvariantId()
    {
        // Arrange
        const string id = " SampleId ";
        string expectedId = id.Trim().ToLowerInvariant();
        var duration = TimeSpan.FromSeconds(5);
        var config = new SessionConfig(
            id,
            TargetCycles: 1,
            DelayBetweenTimes: duration,
            FocusDuration: duration,
            BreakDuration: duration);

        // Assert
        Assert.Equal(expectedId, config.NormalizedId);
    }
}