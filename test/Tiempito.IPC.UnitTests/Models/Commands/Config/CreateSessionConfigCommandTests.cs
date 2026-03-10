using Tiempito.IPC.Models.Commands.Config;

namespace Tiempito.IPC.UnitTests.Models.Commands.Config;

/// <summary>
/// Unit tests for the <see cref="CreateSessionConfigCommand"/> record.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "IPC")]
public sealed class CreateSessionConfigCommandTests
{
    /// <summary>
    /// Tests that <see cref="CreateSessionConfigCommand.CreateNew"/> returns a command
    /// with the specified parameters.
    /// </summary>
    /// <param name="sessionConfigId">The session configuration identifier.</param>
    /// <param name="targetCycles">The target number of cycles.</param>
    /// <param name="focusDuration">The focus duration string.</param>
    /// <param name="breakDuration">The break duration string.</param>
    /// <param name="delayBetweenTimes">The delay between times string, or null.</param>
    [Theory]
    [InlineData("1", 1, "5m", "2.5m", "30s")]
    [InlineData("2", 5, "25m", "2m", "15s")]
    [InlineData("3", 2, "12m", "5m", "45m")]
    [InlineData("4", 0, "1m", "25m", "5s")]
    public void CreateNew_should_ReturnCommandWithSpecifiedParameters(
        string sessionConfigId,
        uint targetCycles,
        string focusDuration,
        string breakDuration,
        string? delayBetweenTimes)
    {
        // Act
        var actual = CreateSessionConfigCommand
            .CreateNew(sessionConfigId, targetCycles, focusDuration, breakDuration, delayBetweenTimes);

        // Assert
        Assert.Equal(sessionConfigId, actual.SessionConfigId);
        Assert.Equal(targetCycles, actual.TargetCycles);
        Assert.Equal(focusDuration, actual.FocusDuration);
        Assert.Equal(breakDuration, actual.BreakDuration);
        Assert.Equal(delayBetweenTimes, actual.DelayBetweenTimes);
    }

    /// <summary>
    /// Tests that <see cref="CreateSessionConfigCommand.CreateNew"/> returns a command
    /// with zero seconds for delay when the delay parameter is null.
    /// </summary>
    [Fact]
    public void CreateNew_should_ReturnCommandWithZeroSecondsForDelay_when_DelayParameterIsNull()
    {
        // Arrange
        const string configId = "1";
        const uint targetCycles = 1;
        const string focusDuration = "5m";
        const string breakDuration = "2.5m";
        const string expectedDelay = "0s";

        // Act
        var actual = CreateSessionConfigCommand
            .CreateNew(configId, targetCycles, focusDuration, breakDuration, delayBetweenTimes: null);

        // Assert
        Assert.Equal(configId, actual.SessionConfigId);
        Assert.Equal(targetCycles, actual.TargetCycles);
        Assert.Equal(focusDuration, actual.FocusDuration);
        Assert.Equal(breakDuration, actual.BreakDuration);
        Assert.Equal(expectedDelay, actual.DelayBetweenTimes);
    }

    /// <summary>
    /// Tests that <see cref="CreateSessionConfigCommand.CreateNew"/> throws an <see cref="ArgumentNullException"/>
    /// when the session configuration identifier is null.
    /// </summary>
    [Fact]
    public void CreateNew_should_ThrowArgumentNullException_when_SessionConfigIdIsNull()
    {
        // Arrange
        const uint targetCycles = 1;
        const string focusDuration = "5m";
        const string breakDuration = "2.5m";
        const string delayBetweenTimes = "30s";

        // Assert
        Assert.Throws<ArgumentNullException>(() => CreateSessionConfigCommand
            .CreateNew(sessionConfigId: null!, targetCycles, focusDuration, breakDuration, delayBetweenTimes));
    }

    /// <summary>
    /// Tests that <see cref="CreateSessionConfigCommand.CreateNew"/> throws an <see cref="ArgumentNullException"/>
    /// when the focus duration parameter is null.
    /// </summary>
    [Fact]
    public void CreateNew_should_ThrowArgumentNullException_when_FocusDurationIsNull()
    {
        // Arrange
        const string configId = "1";
        const uint targetCycles = 1;
        const string breakDuration = "2.5m";
        const string delayBetweenTimes = "30s";

        // Assert
        Assert.Throws<ArgumentNullException>(() => CreateSessionConfigCommand
            .CreateNew(configId, targetCycles, null!, breakDuration, delayBetweenTimes));
    }

    /// <summary>
    /// Tests that <see cref="CreateSessionConfigCommand.CreateNew"/> throws an <see cref="ArgumentNullException"/>
    /// when the break duration parameter is null.
    /// </summary>
    [Fact]
    public void CreateNew_should_ThrowArgumentNullException_when_BreakDurationIsNull()
    {
        // Arrange
        const string configId = "1";
        const uint targetCycles = 1;
        const string focusDuration = "5m";
        const string delayBetweenTimes = "30s";

        // Assert
        Assert.Throws<ArgumentNullException>(() => CreateSessionConfigCommand
            .CreateNew(configId, targetCycles, focusDuration, null!, delayBetweenTimes));
    }
}