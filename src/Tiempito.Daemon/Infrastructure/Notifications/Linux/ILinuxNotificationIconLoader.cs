#if LINUX
namespace Tiempito.Daemon.Infrastructure.Notifications.Linux;

/// <summary>
/// Defines a loader of application's icon.
/// </summary>
internal interface ILinuxNotificationIconLoader
{
    /// <summary>
    /// Loads an icon image into a <see cref="LinuxNotificationImageData"/> struct suitable
    /// for Linux notification D-Bus.
    /// SVG not supported.
    /// </summary>
    /// <param name="iconPath">Path of the icon to load.</param>
    /// <returns>A <see cref="LinuxNotificationImageData"/> to be send in hints of the linux D-Bus interface.</returns>
    public Task<LinuxNotificationImageData> LoadAsync(string iconPath);
}
#endif
