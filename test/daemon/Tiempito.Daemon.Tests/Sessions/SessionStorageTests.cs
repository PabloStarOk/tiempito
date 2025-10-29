using Microsoft.Extensions.Logging;
using Moq;

using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Sessions;
using Tiempito.Daemon.Domain.Sessions.Enums;
using Tiempito.Daemon.Infrastructure.Sessions;

namespace Tiempito.Daemon.Tests.Sessions;

[Trait("Sessions", "Unit")]
public class SessionStorageTests
{
    private readonly SessionStorage _sessionStorage;
    private Session _session;

    public SessionStorageTests()
    {
        var loggerMock = new Mock<ILogger<SessionStorage>>();
        _sessionStorage = new SessionStorage(loggerMock.Object);

        var sessionConfig = new SessionConfig(
            Id: "TestConfig",
            TargetCycles: 1,
            DelayBetweenTimes: TimeSpan.Zero,
            FocusDuration: TimeSpan.Zero,
            BreakDuration: TimeSpan.Zero);
        _session = Session.Create(
            id: "TestSession",
            sessionConfig,
            TimeProvider.System,
            _ => { },
            _ => { },
            _ => { });
    }
    
    [Theory]
    [InlineData(SessionStatus.Executing)]
    [InlineData(SessionStatus.Paused)]
    [InlineData(SessionStatus.Cancelled)]
    [InlineData(SessionStatus.Finished)]
    public void AddSession_should_AddSession(
        SessionStatus status)
    {
        // Act
        bool result = _sessionStorage.AddSession(status, _session);
        
        // Assert
        IReadOnlyDictionary<string, Session> dictionary = GetDictionary(status);
        Assert.True(result);
        Assert.True(dictionary.ContainsKey(_session.Id), "Session was not added to the right dictionary.");
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
        var sessionConfig = new SessionConfig(
            Id: "TestConfig",
            TargetCycles: 20,
            DelayBetweenTimes: time,
            FocusDuration: time,
            BreakDuration: time);
        var newSession = Session.Create(
            _session.Id,
            sessionConfig,
            TimeProvider.System,
            _ => { },
            _ => { },
            _ => { });
        
        // Act
        bool result = _sessionStorage.AddSession(status, newSession);
        
        // Assert
        IReadOnlyDictionary<string, Session> dictionary = GetDictionary(status);
        Assert.False(result);
        Assert.NotEqual(dictionary[_session.Id].Configuration, newSession.Configuration);
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
