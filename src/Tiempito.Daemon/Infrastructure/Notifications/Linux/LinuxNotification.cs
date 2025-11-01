#if LINUX
using Tmds.DBus.Protocol;

namespace Tiempito.Daemon.Infrastructure.Notifications.Linux;

/// <summary>
/// Represents a notification to be sent on Linux systems.
/// </summary>
internal sealed record LinuxNotification
{
    /// <summary>
    /// Gets the name of the application sending the notification.
    /// </summary>
    public string ApplicationName { get; init; }

    /// <summary>
    /// Gets the ID of the notification to be replaced.
    /// </summary>
    public uint ReplacesId { get; init; }

    /// <summary>
    /// Gets the icon associated with the notification.
    /// </summary>
    public string Icon { get; init; }

    /// <summary>
    /// Gets the summary text of the notification.
    /// </summary>
    public string Summary { get; init; }

    /// <summary>
    /// Gets the body text of the notification.
    /// </summary>
    public string Body { get; init; }

    /// <summary>
    /// Gets the actions available for the notification.
    /// </summary>
    public string[] Actions { get; init; }

    /// <summary>
    /// Gets the hints for the notification, as key-value pairs.
    /// </summary>
    public Dictionary<string, VariantValue> Hints { get; init;  }

    /// <summary>
    /// Gets the expiration timeout for the notification in milliseconds.
    /// </summary>
    public int ExpirationTimeout { get; init; }

    /// <summary>
    /// Gets the path to the audio file to be played with the notification.
    /// </summary>
    public string AudioFilePath { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinuxNotification"/> class.
    /// </summary>
    /// <param name="applicationName">The name of the application sending the notification.</param>
    /// <param name="summary">The summary text of the notification.</param>
    /// <param name="body">The body text of the notification.</param>
    /// <param name="replacesId">The ID of the notification to be replaced.</param>
    /// <param name="icon">The icon associated with the notification.</param>
    /// <param name="actions">The actions available for the notification.</param>
    /// <param name="hints">The hints for the notification, as key-value pairs.</param>
    /// <param name="expirationTimeout">The expiration timeout for the notification in milliseconds.</param>
    /// <param name="audioFilePath">The path to the audio file to be played with the notification.</param>
    public LinuxNotification(
        string applicationName,
        string summary = "",
        string body = "",
        uint replacesId = 0,
        string icon = "",
        string[]? actions = null,
        Dictionary<string, VariantValue>? hints = null,
        int expirationTimeout = 0,
        string audioFilePath = "")
    {
        ApplicationName = applicationName;
        ReplacesId = replacesId;
        Icon = icon;
        Summary = summary;
        Body = body;
        Actions = actions ?? [];
        Hints = hints ?? [];
        ExpirationTimeout = expirationTimeout;
        AudioFilePath = audioFilePath;
    }
}
#endif