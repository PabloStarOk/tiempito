using Microsoft.Extensions.Logging;

using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Domain.Sessions;
using Tiempito.Daemon.Infrastructure.Sessions;

namespace Tiempito.Daemon.Tests.Sessions.Helpers;

/// <summary>
/// Creates sessions for tests.
/// </summary>
public static class SessionProvider
{
    public static LoggerFactory LoggerFactory = new ();

    /// <summary>
    /// Creates a configuration with fixed data.
    /// </summary>
    /// <param name="id">ID of the config, "TestConfig" by default.</param>
    /// <returns>A <see cref="SessionConfig"/>.</returns>
    public static SessionConfig CreateConfig(string id = "TestConfig")
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(10);
        
        return new SessionConfig(id.ToLower(), 1, timeSpan, timeSpan, timeSpan);
    }

    /// <summary>
    /// Creates a configuration with random data.
    /// </summary>
    /// <param name="id">ID of the config, "TestConfig" by default.</param>
    /// <returns>A <see cref="SessionConfig"/>.</returns>
    public static SessionConfig CreateRandomConfig(string id = "TestConfig")
    {
        Random random = Random.Shared;

        return new SessionConfig(
            id.ToLower(),
            TargetCycles: random.Next(1, 20),
            DelayBetweenTimes: TimeSpan.FromSeconds(random.Next(0, int.MaxValue)),
            FocusDuration: TimeSpan.FromSeconds(random.Next(1, int.MaxValue)),
            BreakDuration: TimeSpan.FromSeconds(random.Next(1, int.MaxValue)));
    }
    
    /// <summary>
    /// Creates a session with the specified ID and configuration.
    /// </summary>
    /// <param name="id">ID of the session, "TestSession" by default.</param>
    /// <param name="config">Optional session configuration. If null, a default configuration is used.</param>
    /// <returns>A <see cref="Session"/>.</returns>
    public static Session Create(string id = "TestSession", SessionConfig? config = null)
    {
        config ??= CreateConfig();
        var periodicTimer = new PeriodicTimer(Session.SecondInterval, TimeProvider.System);
        var timerWrapper = new DefaultPeriodicTimer(periodicTimer);
        return Session.Create(LoggerFactory.CreateLogger<Session>(), id.ToLower(), config, timerWrapper);
    }

    /// <summary>
    /// Creates a session with random data.
    /// </summary>
    /// <param name="id">ID of the session, "TestSession" by default.</param>
    /// <returns>A <see cref="Session"/>.</returns>
    public static Session CreateRandom(string id = "TestSession")
    {
        Random random = Random.Shared;

        var sessionConfig = new SessionConfig(
            id.ToLower(),
            TargetCycles: random.Next(1, 20),
            DelayBetweenTimes: TimeSpan.FromSeconds(random.Next(0, int.MaxValue)),
            FocusDuration: TimeSpan.FromSeconds(random.Next(1, int.MaxValue)),
            BreakDuration: TimeSpan.FromSeconds(random.Next(1, int.MaxValue)));
        var periodicTimer = new PeriodicTimer(Session.SecondInterval, TimeProvider.System);
        var timerWrapper = new DefaultPeriodicTimer(periodicTimer);
        return Session.Create(LoggerFactory.CreateLogger<Session>(), id.ToLower(), sessionConfig, timerWrapper);
    }
}
