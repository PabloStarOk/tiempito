#if LINUX
namespace Tiempito.Daemon.Infrastructure.Notifications.Linux;

/// <summary>
/// Defines a player of sounds.
/// </summary>
public interface ILinuxSoundPlayer
{
    /// <summary>
    /// Plays an audio file.
    /// </summary>
    /// <param name="filepath">Audio file path to play.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public ValueTask PlayAsync(string filepath);

    /// <summary>
    /// Stop the last played sound.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public ValueTask StopAsync();
}
#endif