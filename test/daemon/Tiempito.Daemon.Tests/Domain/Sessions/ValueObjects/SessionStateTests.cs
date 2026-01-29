using Tiempito.Daemon.Domain.Sessions.Enums;
using Tiempito.Daemon.Domain.Sessions.ValueObjects;

namespace Tiempito.Daemon.Tests.Domain.Sessions.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="SessionState"/> class.
/// </summary>
[Trait("Sessions", "Unit")]
public sealed class SessionStateTests
{
    private readonly SessionState _state;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionStateTests"/> class.
    /// </summary>
    public SessionStateTests()
    {
        _state = SessionState.CreateInitial(TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Tests that <see cref="SessionState.CreateInitial"/> creates a state with the expected initial data.
    /// </summary>
    [Fact]
    public void CreateInitial_should_StateWithInitialData()
    {
        // Arrange
        var targetDuration = TimeSpan.FromSeconds(5);

        // Act
        var actual = SessionState.CreateInitial(targetDuration);

        // Assert
        Assert.Equal(TimeSpan.Zero, actual.ElapsedTime);
        Assert.Equal(targetDuration, actual.TargetDuration);
        Assert.Equal(0, actual.Cycle);
        Assert.Equal(SessionIntervalType.Focus, actual.IntervalType);
        Assert.Equal(SessionStatus.None, actual.Status);
    }

    /// <summary>
    /// Tests that <see cref="SessionState.WithElapsedSecond"/> increases the elapsed time by one second.
    /// </summary>
    [Fact]
    public void WithElapsedSecond_should_ElapseOneSecond()
    {
        // Act
        var actual = _state.WithElapsedSecond();

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(1), actual.ElapsedTime);
    }

    /// <summary>
    /// Tests that <see cref="SessionState.WithStatus"/> changes the session status to the specified value.
    /// </summary>
    /// <param name="expectedStatus">The status to set on the session state.</param>
    [Theory]
    [InlineData(SessionStatus.None)]
    [InlineData(SessionStatus.Executing)]
    [InlineData(SessionStatus.Paused)]
    [InlineData(SessionStatus.Cancelled)]
    [InlineData(SessionStatus.Finished)]
    public void WithStatus_should_ChangeToSpecifiedStatus(SessionStatus expectedStatus)
    {
        // Act
        var actual = _state.WithStatus(expectedStatus);

        // Assert
        Assert.Equal(expectedStatus, actual.Status);
    }

    /// <summary>
    /// Tests that <see cref="SessionState.WithNewInterval"/> changes the interval type and target duration as specified.
    /// </summary>
    /// <param name="expectedIntervalType">The interval type to set on the session state.</param>
    [Theory]
    [InlineData(SessionIntervalType.Focus)]
    [InlineData(SessionIntervalType.Break)]
    [InlineData(SessionIntervalType.Delay)]
    public void WithNewInterval_should_ChangeToSpecifiedIntervalAndDuration(SessionIntervalType expectedIntervalType)
    {
        // Arrange
        var expectedTargetDuration = TimeSpan.FromSeconds(10);
        var state = SessionState.CreateInitial(TimeSpan.FromSeconds(1));
        var updatedState = state.WithElapsedSecond();

        // Act
        var actual = updatedState.WithNewInterval(expectedIntervalType, expectedTargetDuration);

        // Assert
        Assert.Equal(expectedIntervalType, actual.IntervalType);
        Assert.Equal(expectedTargetDuration, actual.TargetDuration);
    }
}
