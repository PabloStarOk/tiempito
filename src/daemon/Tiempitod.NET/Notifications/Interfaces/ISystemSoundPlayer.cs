namespace Tiempitod.NET.Notifications.Interfaces;

/// <summary>
/// Defines a player of sounds.
/// </summary>
public interface ISystemSoundPlayer : IDisposable
{
    /// <summary>
    /// Plays an audio file.
    /// </summary>
    /// <param name="filepath">Audio file path to play.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task PlayAsync(string filepath);
    
    /// <summary>
    /// Stop the last played sound.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StopAsync();
}
