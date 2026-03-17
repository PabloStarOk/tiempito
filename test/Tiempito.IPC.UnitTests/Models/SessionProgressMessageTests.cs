using Tiempito.IPC.Models;
using Tiempito.IPC.Models.Enums;

namespace Tiempito.IPC.UnitTests.Models;

/// <summary>
/// Unit tests for the <see cref="SessionProgressMessage"/> record.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "IPC")]
public sealed class SessionProgressMessageTests
{
    /// <summary>
    /// Provides test data for the <see cref="SessionProgressMessage.CreateNew"/> method.
    /// Each entry contains:
    /// <list type="bullet">
    ///   <item><description><c>sessionId</c>: The session identifier.</description></item>
    ///   <item><description><c>intervalDuration</c>: The duration of the interval.</description></item>
    ///   <item><description><c>intervalType</c>: The type of the interval.</description></item>
    ///   <item><description><c>cycle</c>: The cycle number.</description></item>
    ///   <item><description><c>elapsedTime</c>: The elapsed time in the interval.</description></item>
    ///   <item><description><c>sessionCompleted</c>: Indicates whether the session is completed.</description></item>
    /// </list>
    /// </summary>
    /// <returns>
    /// A <see cref="TheoryData{T1,T2,T3,T4,T5,T6}"/> containing test cases for <c>SessionProgressMessage.CreateNew</c>.
    /// </returns>
    public static TheoryData<string, TimeSpan, SessionIntervalType, int, TimeSpan, bool> CreateNewData()
    {
        return new TheoryData<string, TimeSpan, SessionIntervalType, int, TimeSpan, bool>
        {
            { "1", TimeSpan.FromMinutes(25), SessionIntervalType.Focus, 0, TimeSpan.FromMinutes(10), false },
            { "2", TimeSpan.FromMinutes(5), SessionIntervalType.Break, 1, TimeSpan.FromMinutes(5), false },
            { "3", TimeSpan.FromMinutes(2), SessionIntervalType.Delay, 4, TimeSpan.FromMinutes(1), false },
            { "4", TimeSpan.FromMinutes(25), SessionIntervalType.Focus, 2, TimeSpan.Zero, false },
            { "5", TimeSpan.FromMinutes(30), SessionIntervalType.Focus, 0, TimeSpan.FromMinutes(30), true },
        };
    }

    /// <summary>
    /// Tests that <see cref="SessionProgressMessage.CreateNew"/> returns a message with the specified parameters.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="intervalDuration">The duration of the interval.</param>
    /// <param name="intervalType">The type of the interval.</param>
    /// <param name="cycle">The cycle number.</param>
    /// <param name="elapsedTime">The elapsed time in the interval.</param>
    /// <param name="sessionCompleted">Indicates whether the session is completed.</param>
    [Theory]
    [MemberData(nameof(CreateNewData))]
    public void CreateNew_should_ReturnExpectedMessageWithSpecifiedParameters(
        string sessionId,
        TimeSpan intervalDuration,
        SessionIntervalType intervalType,
        int cycle,
        TimeSpan elapsedTime,
        bool sessionCompleted)
    {
        // Arrange
        bool intervalCompleted = intervalDuration >= elapsedTime;

        // Act
        var actual = SessionProgressMessage.CreateNew(
            sessionId,
            intervalDuration,
            intervalType,
            cycle,
            elapsedTime,
            intervalCompleted,
            sessionCompleted);

        // Assert
        Assert.Equal(sessionId, actual.SessionId);
        Assert.Equal(intervalDuration, actual.IntervalDuration);
        Assert.Equal(intervalType, actual.IntervalType);
        Assert.Equal(cycle, actual.Cycle);
        Assert.Equal(elapsedTime, actual.ElapsedTime);
        Assert.Equal(intervalCompleted, actual.IntervalCompleted);
        Assert.Equal(sessionCompleted, actual.SessionCompleted);
    }

    /// <summary>
    /// Tests that <see cref="SessionProgressMessage.CreateNew"/> creates messages with random UUIDs for ID and CorrelationId.
    /// </summary>
    [Fact]
    public void CreateNew_should_CreateMessagesWithRandomUuid()
    {
        // Arrange
        const string sessionId = "1";
        TimeSpan intervalDuration = TimeSpan.FromMinutes(2);
        const SessionIntervalType intervalType = SessionIntervalType.Focus;
        const int cycle = 2;
        TimeSpan elapsedTime = TimeSpan.FromMinutes(1);
        bool intervalCompleted = intervalDuration == elapsedTime;
        const bool sessionCompleted = false;

        // Act
        var msg1 = SessionProgressMessage.CreateNew(
            sessionId, intervalDuration, intervalType, cycle, elapsedTime, intervalCompleted, sessionCompleted);
        var msg2 = SessionProgressMessage.CreateNew(
            sessionId, intervalDuration, intervalType, cycle, elapsedTime, intervalCompleted, sessionCompleted);

        // Assert
        Assert.NotEqual(msg1.Id, msg2.Id);
        Assert.NotEqual(msg1.CorrelationId, msg2.CorrelationId);
    }
}