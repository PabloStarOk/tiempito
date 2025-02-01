using Tiempito.Daemon.Configuration.Session.Objects;
using Tiempito.Daemon.Sessions.Objects;

namespace Tiempito.Daemon.Tests.Sessions.Helpers;

/// <summary>
/// Creates sessions for tests.
/// </summary>
public static class SessionProvider
{
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

        return new SessionConfig
        (
            id.ToLower(),
            targetCycles: random.Next(1, 20),
            delayBetweenTimes: TimeSpan.FromSeconds(random.Next(0, int.MaxValue)),
            focusDuration: TimeSpan.FromSeconds(random.Next(1, int.MaxValue)),
            breakDuration: TimeSpan.FromSeconds(random.Next(1, int.MaxValue)));
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

        return new Session(
            id.ToLower(),
            config.Value.TargetCycles,
            config.Value.DelayBetweenTimes,
            config.Value.FocusDuration,
            config.Value.BreakDuration);
    }

    /// <summary>
    /// Creates a session with random data.
    /// </summary>
    /// <param name="id">ID of the session, "TestSession" by default.</param>
    /// <returns>A <see cref="Session"/>.</returns>
    public static Session CreateRandom(string id = "TestSession")
    {
        Random random = Random.Shared;

        return new Session
        (
            id.ToLower(),
            targetCycles: random.Next(1, 20),
            delayBetweenTimes: TimeSpan.FromSeconds(random.Next(0, int.MaxValue)),
            focusDuration: TimeSpan.FromSeconds(random.Next(1, int.MaxValue)),
            breakDuration: TimeSpan.FromSeconds(random.Next(1, int.MaxValue)));
    }
}
