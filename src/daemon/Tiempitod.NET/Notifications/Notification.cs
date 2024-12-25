namespace Tiempitod.NET.Notifications;

[Serializable]
public struct Notification
{
    public string ApplicationName { get; set; }
    public uint ReplacesId { get; set; }
    public string Icon { get; set; }
    public string Summary { get; set; }
    public string Body { get; set;  }
    public string[] Actions { get; set; }
    public IDictionary<string, object> Hints { get; set;  }
    public int ExpirationTimeout { get; set; }

    public Notification(string applicationName,
        string summary,
        string body,
        uint replacesId = 0,
        string icon = "",
        string[]? actions = default,
        IDictionary<string, object>? hints = default,
        int expirationTimeout = 0)
    {
        ApplicationName = applicationName;
        ReplacesId = replacesId;
        Icon = icon;
        Summary = summary;
        Body = body;
        Actions = actions ?? [];
        Hints = hints ?? new Dictionary<string, object>();
        ExpirationTimeout = expirationTimeout;
    }
}
