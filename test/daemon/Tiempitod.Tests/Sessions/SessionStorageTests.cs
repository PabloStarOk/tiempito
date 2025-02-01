using Microsoft.Extensions.Logging;
using Moq;
using Tiempitod.NET.Sessions;
using Tiempitod.NET.Sessions.Enums;
using Tiempitod.NET.Sessions.Objects;

namespace Tiempitod.Tests.Sessions;

[Trait("Sessions", "Unit")]
public class SessionStorageTests
{
    private readonly SessionStorage _sessionStorage;
    private Session _session;

    public SessionStorageTests()
    {
        var loggerMock = new Mock<ILogger<SessionStorage>>();
        _sessionStorage = new SessionStorage(loggerMock.Object);
        
        _session = new Session(id: "TestSession",
            targetCycles: 1, delayBetweenTimes: TimeSpan.Zero,
            focusDuration: TimeSpan.Zero, breakDuration: TimeSpan.Zero);
    }
    
    [Theory]
    [InlineData(SessionStatus.Executing)]
    [InlineData(SessionStatus.Paused)]
    [InlineData(SessionStatus.Cancelled)]
    [InlineData(SessionStatus.Finished)]
    public void AddSession_should_AddAndChangeItsStatus(
        SessionStatus status)
    {
        // Act
        bool result = _sessionStorage.AddSession(status, _session);
        
        // Assert
        IReadOnlyDictionary<string, Session> dictionary = GetDictionary(status);
        Assert.True(result);
        Assert.True(dictionary.ContainsKey(_session.Id),
               "Session was not added to the right dictionary.");
        Assert.Equal(dictionary[_session.Id].Status, status);
    }
    
    [Theory]
    [InlineData(SessionStatus.Executing)]
    [InlineData(SessionStatus.Paused)]
    [InlineData(SessionStatus.Cancelled)]
    [InlineData(SessionStatus.Finished)]
    public void AddSession_should_NotAddAndChangeItsStatus_when_SessionWithSameIdExists(
        SessionStatus status)
    {
        // Arrange
        _sessionStorage.AddSession(status, _session);
        TimeSpan time = TimeSpan.FromSeconds(10);
        var newSession = new Session(_session.Id, 20, time, time, time);
        
        // Act
        bool result = _sessionStorage.AddSession(status, newSession);
        
        // Assert
        IReadOnlyDictionary<string, Session> dictionary = GetDictionary(status);
        Assert.False(result);
        Assert.NotEqual(dictionary[_session.Id].TargetCycles, newSession.TargetCycles);
        Assert.NotEqual(dictionary[_session.Id].DelayBetweenTimes, newSession.DelayBetweenTimes);
        Assert.NotEqual(dictionary[_session.Id].FocusDuration, newSession.FocusDuration);
        Assert.NotEqual(dictionary[_session.Id].BreakDuration, newSession.BreakDuration);
    }
    
    [Theory]
    [InlineData(SessionStatus.Executing)]
    [InlineData(SessionStatus.Paused)]
    [InlineData(SessionStatus.Cancelled)]
    [InlineData(SessionStatus.Finished)]
    public void UpdateSession_should_Update_when_SessionExistsInTheDictionary(
        SessionStatus status)
    {
        // Arrange
        int newElapsedTime = GenerateNonZeroInt();
        _sessionStorage.AddSession(status, _session);
        
        // Act
        _session.Elapsed += TimeSpan.FromSeconds(newElapsedTime);
        _sessionStorage.UpdateSession(status, _session);
        
        // Assert
        IReadOnlyDictionary<string, Session> dictionary = GetDictionary(status);
        Assert.Equal(dictionary[_session.Id].Elapsed,
            TimeSpan.FromSeconds(newElapsedTime));
    }
    
    [Theory]
    [MemberData(nameof(InvalidStatusPairs))]
    public void UpdateSession_should_NotUpdate_when_SessionNotExistsInTheDictionary(
        SessionStatus status, SessionStatus falseStatus)
    {
        // Arrange
        int newElapsedTime = GenerateNonZeroInt();
        _sessionStorage.AddSession(status, _session);
        
        // Act
        _session.Elapsed += TimeSpan.FromSeconds(newElapsedTime);
        _sessionStorage.UpdateSession(falseStatus, _session);
        
        // Assert
        IReadOnlyDictionary<string, Session> dictionary = GetDictionary(status);
        Assert.NotEqual(dictionary[_session.Id].Elapsed, TimeSpan.FromSeconds(newElapsedTime));
    }

    [Theory]
    [InlineData(SessionStatus.Executing)]
    [InlineData(SessionStatus.Paused)]
    [InlineData(SessionStatus.Cancelled)]
    [InlineData(SessionStatus.Finished)]
    public void RemoveSession_should_Remove(
        SessionStatus status)
    {
        // Arrange
        _sessionStorage.AddSession(status, _session);
        
        // Act
        _sessionStorage.RemoveSession(status, _session.Id);
        
        // Assert
        IReadOnlyDictionary<string, Session> dictionary = GetDictionary(status);
        Assert.False(dictionary.ContainsKey(_session.Id), "Session was not removed.");
    }
    
    /// <summary>
    /// Returns a random integer with a minimum value of 1.
    /// </summary>
    /// <returns>An integer.</returns>
    private static int GenerateNonZeroInt() => Random.Shared.Next(1, int.MaxValue);
    
    /// <summary>
    /// Get a pair of <see cref="SessionStatus"/> that excludes mutually.
    /// </summary>
    /// <returns>An object with two <see cref="SessionStatus"/>.</returns>
    public static IEnumerable<object[]> InvalidStatusPairs()
    {
        SessionStatus[] allStatuses = Enum.GetValues(typeof(SessionStatus)).Cast<SessionStatus>().ToArray();
        foreach (SessionStatus original in allStatuses)
        {
            if (original is SessionStatus.None)
                continue;
            
            foreach (SessionStatus invalid in allStatuses.
                         Where(s => s != original && s is not SessionStatus.None))
            {
                yield return [original, invalid];
            }
        }
    }
    
    /// <summary>
    /// Gets the dictionary of the session storage.
    /// </summary>
    /// <param name="status">Status of the session.</param>
    /// <returns>A <see cref="IReadOnlyDictionary{TKey,TValue}"/>.</returns>
    /// <exception cref="NotImplementedException">If the given status is <see cref="SessionStatus.None"/>.</exception>
    private IReadOnlyDictionary<string, Session> GetDictionary(SessionStatus status)
    {
        return status switch
        {
            SessionStatus.Executing => _sessionStorage.RunningSessions,
            SessionStatus.Cancelled => _sessionStorage.CancelledSessions,
            SessionStatus.Paused => _sessionStorage.PausedSessions,
            SessionStatus.Finished => _sessionStorage.FinishedSessions,
            _ => throw new NotImplementedException("Passed a wrong data case.")
        };
    }
}
