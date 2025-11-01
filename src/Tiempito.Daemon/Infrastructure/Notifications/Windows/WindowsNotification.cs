#if WINDOWS10_0_17763_0_OR_GREATER
namespace Tiempito.Daemon.Infrastructure.Notifications.Windows;

/// <summary>
/// Represents a Windows notification with header, body, icon, and expiration time.
/// </summary>
/// <param name="Header">The title of the notification.</param>
/// <param name="Body">The message content of the notification.</param>
/// <param name="IconFilePath">The file path to the icon displayed in the notification.</param>
/// <param name="ExpirationTime">The time in seconds before the notification expires.</param>
internal sealed record WindowsNotification(
    string Header,
    string Body,
    Uri IconFilePath,
    int ExpirationTime);
#endif