using Tmds.DBus.Protocol;

namespace Tiempito.Daemon.Notifications.Objects;

/// <summary>
/// Represents a notification.
/// </summary>
public struct Notification
{
    public string ApplicationName { get; set; }
    public uint ReplacesId { get; set; }
    public string Icon { get; set; }
    public string Summary { get; set; }
    public string Body { get; set;  }
    public string[] Actions { get; set; }
    public Dictionary<string, VariantValue> Hints { get; set;  }
    public int ExpirationTimeout { get; set; }
    public string AudioFilePath { get; set; }

    public Notification(string applicationName,
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
        Hints = hints ?? new Dictionary<string, VariantValue>();
        ExpirationTimeout = expirationTimeout;
        AudioFilePath = audioFilePath;
    }
}
