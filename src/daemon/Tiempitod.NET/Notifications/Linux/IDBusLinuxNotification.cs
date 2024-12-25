using Tmds.DBus;

namespace Tiempitod.NET.Notifications.Linux;

[DBusInterface("org.freedesktop.Notifications")]
public interface IDBusLinuxNotification : IDBusObject
{
    public Task<uint> NotifyAsync(string appName, uint replacesId, string appIcon, string summary, string body, string[] actions, IDictionary<string, object> hints, int expireTimeout);
    
    public Task CloseNotificationAsync(uint id);
}
