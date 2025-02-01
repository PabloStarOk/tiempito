using Tiempito.Daemon.Notifications.Objects;

namespace Tiempito.Daemon.Notifications.Interfaces;

/// <summary>
/// Defines a loader of application's icon.
/// </summary>
public interface ISystemAsyncIconLoader
{
    /// <summary>
    /// Loads an icon image into a <see cref="NotificationImageData"/> struct suitable
    /// for Linux notification D-Bus.
    /// SVG not supported.
    /// </summary>
    /// <param name="iconPath">Path of the icon to load.</param>
    /// <returns>A <see cref="NotificationImageData"/> to be send in hints of the linux D-Bus interface.</returns>
    public Task<NotificationImageData> LoadAsync(string iconPath);
}
