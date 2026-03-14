using Tiempito.IPC.Models.Commands.Config;

namespace Tiempito.IPC.UnitTests.Models.Commands.Config;

/// <summary>
/// Unit tests for the <see cref="ModifySessionConfigCommand"/> record.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "IPC")]
public sealed class ModifySessionConfigCommandTests
{
    /// <summary>
    /// Tests that <see cref="ModifySessionConfigCommand.CreateNew"/> returns a command
    /// with the specified parameters.
    /// </summary>
    /// <param name="sessionConfigId">The session configuration identifier.</param>
    /// <param name="targetCycles">The target number of cycles, or null.</param>
    /// <param name="focusDuration">The focus duration string, or null.</param>
    /// <param name="breakDuration">The break duration string, or null.</param>
    /// <param name="delayBetweenTimes">The delay between times string, or null.</param>
    [Theory]
    [InlineData("1", 1, "5m", "2.5m", "30s")]
    [InlineData("2", 5, "25m", "2m", "15s")]
    [InlineData("3", 2, "12m", "5m", "45m")]
    [InlineData("4", 0, "1m", "25m", "5s")]
    [InlineData("5", null, "5m", "2.5m", "30s")]
    [InlineData("6", 5, null, "2m", "15s")]
    [InlineData("7", 2, "12m", null, "45m")]
    [InlineData("8", 0, "1m", "25m", null)]
    [InlineData("9", null, null, null, null)]
    public void CreateNew_should_ReturnCommandWithSpecifiedParameters(
        string sessionConfigId,
        int? targetCycles,
        string? focusDuration,
        string? breakDuration,
        string? delayBetweenTimes)
    {
        // Arrange
        var targetCyclesUint = (uint?)targetCycles;

        // Act
        var actual = ModifySessionConfigCommand
            .CreateNew(sessionConfigId, targetCyclesUint, focusDuration, breakDuration, delayBetweenTimes);

        // Assert
        Assert.Equal(sessionConfigId, actual.SessionConfigId);
        Assert.Equal(targetCyclesUint, actual.TargetCycles);
        Assert.Equal(focusDuration, actual.FocusDuration);
        Assert.Equal(breakDuration, actual.BreakDuration);
        Assert.Equal(delayBetweenTimes, actual.DelayBetweenTimes);
    }

    /// <summary>
    /// Tests that <see cref="ModifySessionConfigCommand.CreateNew"/> creates commands with random UUIDs for ID and CorrelationId.
    /// </summary>
    [Fact]
    public void CreateNew_should_CreateCommandWithRandomUuid()
    {
        // Act
        var cmd1 = ModifySessionConfigCommand.CreateNew("1", null, null, null, null);
        var cmd2 = ModifySessionConfigCommand.CreateNew("1", null, null, null, null);

        // Assert
        Assert.NotEqual(cmd1.Id, cmd2.Id);
        Assert.NotEqual(cmd1.CorrelationId, cmd2.CorrelationId);
    }

    /// <summary>
    /// Tests that <see cref="ModifySessionConfigCommand.CreateNew"/> throws an <see cref="ArgumentNullException"/>
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
        Assert.Throws<ArgumentNullException>(() => ModifySessionConfigCommand
            .CreateNew(sessionConfigId: null!, targetCycles, focusDuration, breakDuration, delayBetweenTimes));
    }
}