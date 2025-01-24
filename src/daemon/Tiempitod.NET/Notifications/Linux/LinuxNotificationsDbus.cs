using Tmds.DBus.Protocol;

namespace Tiempitod.NET.Notifications.Linux;

// TODO: Move everything to a library.

public record NotificationsProperties
{
    public bool Dnd { get; set; }
}

public class LinuxNotificationsDbus : NotificationsObject
{
    private const string Interface = "org.freedesktop.Notifications";

    public LinuxNotificationsDbus(
        NotificationsService service,
        ObjectPath path) : base(service, path)
    { }

    public Task SetNotiWindowVisibilityAsync(bool value)
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                signature: "b",
                member: "SetNotiWindowVisibility"
            );
            writer.WriteBool(value);
            return writer.CreateMessage();
        }
    }

    public Task<bool> ToggleDndAsync()
    {
        return Connection.CallMethodAsync(CreateMessage(), (m, s) => ReadMessage_b(m, (NotificationsObject)s!), this);

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                member: "ToggleDnd"
            );
            return writer.CreateMessage();
        }
    }

    // public Task SetDndAsync(bool state)
    // {
    //     return this.Connection.CallMethodAsync(CreateMessage());
    //     MessageBuffer CreateMessage()
    //     {
    //         var writer = this.Connection.GetMessageWriter();
    //         writer.WriteMethodCallHeader(
    //             destination: Service.Destination,
    //             path: Path,
    //             @interface: Interface,
    //             signature: "b",
    //             member: "SetDnd");
    //         writer.WriteBool(state);
    //         return writer.CreateMessage();
    //     }
    // }
    // public Task<bool> GetDndAsync()
    // {
    //     return this.Connection.CallMethodAsync(CreateMessage(), (Message m, object? s) => ReadMessage_b(m, (NotificationsObject)s!), this);
    //     MessageBuffer CreateMessage()
    //     {
    //         var writer = this.Connection.GetMessageWriter();
    //         writer.WriteMethodCallHeader(
    //             destination: Service.Destination,
    //             path: Path,
    //             @interface: Interface,
    //             member: "GetDnd");
    //         return writer.CreateMessage();
    //     }
    // }
    
    public Task ManuallyCloseNotificationAsync(uint id, bool timeout)
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                signature: "ub",
                member: "ManuallyCloseNotification"
            );
            writer.WriteUInt32(id);
            writer.WriteBool(timeout);
            return writer.CreateMessage();
        }
    }

    public Task CloseAllNotificationsAsync()
    {
        return this.Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = this.Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                member: "CloseAllNotifications"
            );
            return writer.CreateMessage();
        }
    }

    public Task HideLatestNotificationAsync(bool close)
    {
        return this.Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = this.Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                signature: "b",
                member: "HideLatestNotification"
            );
            writer.WriteBool(close);
            return writer.CreateMessage();
        }
    }

    public Task<string[]> GetCapabilitiesAsync()
    {
        return Connection.CallMethodAsync(CreateMessage(), (m, s) => ReadMessage_as(m, (NotificationsObject)s!), this);

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                member: "GetCapabilities"
            );
            return writer.CreateMessage();
        }
    }

    public Task<uint> NotifyAsync(string appName, uint replacesId, string appIcon, string summary, string body, string[] actions,
        Dictionary<string, VariantValue> hints, int expireTimeout)
    {
        return this.Connection.CallMethodAsync(CreateMessage(), (m, s) => ReadMessage_u(m, (NotificationsObject)s!), this);

        MessageBuffer CreateMessage()
        {
            var writer = this.Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                signature: "susssasa{sv}i",
                member: "Notify"
            );
            writer.WriteString(appName);
            writer.WriteUInt32(replacesId);
            writer.WriteString(appIcon);
            writer.WriteString(summary);
            writer.WriteString(body);
            writer.WriteArray(actions);
            writer.WriteDictionary(hints);
            writer.WriteInt32(expireTimeout);
            return writer.CreateMessage();
        }
    }

    public Task CloseNotificationAsync(uint id)
    {
        return this.Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = this.Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                signature: "u",
                member: "CloseNotification"
            );
            writer.WriteUInt32(id);
            return writer.CreateMessage();
        }
    }

    public Task<(string Name, string Vendor, string Version, string SpecVersion)> GetServerInformationAsync()
    {
        return this.Connection.CallMethodAsync(CreateMessage(), (m, s) => ReadMessage_ssss(m, (NotificationsObject)s!), this);

        MessageBuffer CreateMessage()
        {
            var writer = this.Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                member: "GetServerInformation"
            );
            return writer.CreateMessage();
        }
    }

    public ValueTask<IDisposable> WatchOnDndToggleAsync(Action<Exception?, bool> handler, bool emitOnCapturedContext = true, ObserverFlags flags = ObserverFlags.None)
        => WatchSignalAsync
        (
            Service.Destination, Interface, Path, "OnDndToggle", (m, s) => ReadMessage_b(m, (NotificationsObject)s!), handler, emitOnCapturedContext,
            flags
        );

    public ValueTask<IDisposable> WatchNotificationClosedAsync(Action<Exception?, (uint Id, uint Reason)> handler, bool emitOnCapturedContext = true,
        ObserverFlags flags = ObserverFlags.None)
        => WatchSignalAsync
        (
            Service.Destination, Interface, Path, "NotificationClosed", (m, s) => ReadMessage_uu(m, (NotificationsObject)s!), handler,
            emitOnCapturedContext, flags
        );

    public ValueTask<IDisposable> WatchActionInvokedAsync(Action<Exception?, (uint Id, string ActionKey)> handler, bool emitOnCapturedContext = true,
        ObserverFlags flags = ObserverFlags.None)
        => WatchSignalAsync
        (
            Service.Destination, Interface, Path, "ActionInvoked", (m, s) => ReadMessage_us(m, (NotificationsObject)s!), handler, emitOnCapturedContext,
            flags
        );

    public ValueTask<IDisposable> WatchNotificationRepliedAsync(Action<Exception?, (uint Id, string Text)> handler, bool emitOnCapturedContext = true,
        ObserverFlags flags = ObserverFlags.None)
        => WatchSignalAsync
        (
            Service.Destination, Interface, Path, "NotificationReplied", (m, s) => ReadMessage_us(m, (NotificationsObject)s!), handler,
            emitOnCapturedContext, flags
        );

    public Task SetDndAsync(bool value)
    {
        return this.Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = this.Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: "org.freedesktop.DBus.Properties",
                signature: "ssv",
                member: "Set"
            );
            writer.WriteString(Interface);
            writer.WriteString("Dnd");
            writer.WriteSignature("b");
            writer.WriteBool(value);
            return writer.CreateMessage();
        }
    }

    public Task<bool> GetDndAsync()
        => Connection.CallMethodAsync(CreateGetPropertyMessage(Interface, "Dnd"), (m, s) => ReadMessage_v_b(m, (NotificationsObject)s!), this);

    public Task<NotificationsProperties> GetPropertiesAsync()
    {
        return Connection.CallMethodAsync(CreateGetAllPropertiesMessage(Interface), (m, s) => ReadMessage(m, (NotificationsObject)s!), this);

        static NotificationsProperties ReadMessage(Message message, NotificationsObject _)
        {
            var reader = message.GetBodyReader();
            return ReadProperties(ref reader);
        }
    }

    public ValueTask<IDisposable> WatchPropertiesChangedAsync(Action<Exception?, PropertyChanges<NotificationsProperties>> handler, bool emitOnCapturedContext = true,
        ObserverFlags flags = ObserverFlags.None)
    {
        return base.WatchPropertiesChangedAsync(Interface, (m,  s) => ReadMessage(m, (NotificationsObject)s!), handler, emitOnCapturedContext, flags);

        static PropertyChanges<NotificationsProperties> ReadMessage(Message message, NotificationsObject _)
        {
            var reader = message.GetBodyReader();
            reader.ReadString(); // interface
            List<string> changed = new(), invalidated = new();
            return new PropertyChanges<NotificationsProperties>(ReadProperties(ref reader, changed), ReadInvalidated(ref reader), changed.ToArray());
        }

        static string[] ReadInvalidated(ref Reader reader)
        {
            List<string>? invalidated = null;
            ArrayEnd arrayEnd = reader.ReadArrayStart(DBusType.String);
            while (reader.HasNext(arrayEnd))
            {
                invalidated ??= new();
                var property = reader.ReadString();
                switch (property)
                {
                    case "Dnd": invalidated.Add("Dnd"); break;
                }
            }

            return invalidated?.ToArray() ?? Array.Empty<string>();
        }
    }

    private static NotificationsProperties ReadProperties(ref Reader reader, List<string>? changedList = null)
    {
        var props = new NotificationsProperties();
        ArrayEnd arrayEnd = reader.ReadArrayStart(DBusType.Struct);
        while (reader.HasNext(arrayEnd))
        {
            var property = reader.ReadString();
            switch (property)
            {
                case "Dnd":
                    reader.ReadSignature("b"u8);
                    props.Dnd = reader.ReadBool();
                    changedList?.Add("Dnd");
                    break;
                default:
                    reader.ReadVariantValue();
                    break;
            }
        }

        return props;
    }
}

public record CcProperties
{
    public bool Inhibited { get; set; }
}

public class Cc : NotificationsObject
{
    private const string Interface = "org.erikreider.swaync.cc";

    public Cc(NotificationsService service, ObjectPath path) : base(service, path)
    {
    }

    public Task<(bool, bool, uint, bool)> GetSubscribeDataAsync()
    {
        return this.Connection.CallMethodAsync(CreateMessage(), (m, s) => ReadMessage_rbbubz(m, (NotificationsObject)s!), this);

        MessageBuffer CreateMessage()
        {
            var writer = this.Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                member: "GetSubscribeData"
            );
            return writer.CreateMessage();
        }
    }

    public Task<bool> ReloadCssAsync()
    {
        return this.Connection.CallMethodAsync(CreateMessage(), (m,  s) => ReadMessage_b(m, (NotificationsObject)s!), this);

        MessageBuffer CreateMessage()
        {
            var writer = this.Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                member: "ReloadCss"
            );
            return writer.CreateMessage();
        }
    }

    public Task ReloadConfigAsync()
    {
        return this.Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = this.Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                member: "ReloadConfig"
            );
            return writer.CreateMessage();
        }
    }

    public Task ChangeConfigValueAsync(string name, VariantValue value, bool writeToFile, string path)
    {
        return this.Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = this.Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                signature: "svbs",
                member: "ChangeConfigValue"
            );
            writer.WriteString(name);
            writer.WriteVariant(value);
            writer.WriteBool(writeToFile);
            writer.WriteString(path);
            return writer.CreateMessage();
        }
    }

    public Task<bool> GetVisibilityAsync()
    {
        return this.Connection.CallMethodAsync(CreateMessage(), (m, s) => ReadMessage_b(m, (NotificationsObject)s!), this);

        MessageBuffer CreateMessage()
        {
            var writer = this.Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                member: "GetVisibility"
            );
            return writer.CreateMessage();
        }
    }

    public Task HideLatestNotificationsAsync(bool close)
    {
        return this.Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = this.Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                signature: "b",
                member: "HideLatestNotifications"
            );
            writer.WriteBool(close);
            return writer.CreateMessage();
        }
    }

    public Task CloseAllNotificationsAsync()
    {
        return this.Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = this.Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                member: "CloseAllNotifications"
            );
            return writer.CreateMessage();
        }
    }

    public Task<uint> NotificationCountAsync()
    {
        return this.Connection.CallMethodAsync(CreateMessage(), (m, s) => ReadMessage_u(m, (NotificationsObject)s!), this);

        MessageBuffer CreateMessage()
        {
            var writer = this.Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                member: "NotificationCount"
            );
            return writer.CreateMessage();
        }
    }

    public Task ToggleVisibilityAsync()
    {
        return this.Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = this.Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                member: "ToggleVisibility"
            );
            return writer.CreateMessage();
        }
    }

    public Task SetVisibilityAsync(bool visibility)
    {
        return this.Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = this.Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                signature: "b",
                member: "SetVisibility"
            );
            writer.WriteBool(visibility);
            return writer.CreateMessage();
        }
    }

    public Task<bool> ToggleDndAsync()
    {
        return this.Connection.CallMethodAsync(CreateMessage(), (m, s) => ReadMessage_b(m, (NotificationsObject)s!), this);

        MessageBuffer CreateMessage()
        {
            var writer = this.Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                member: "ToggleDnd"
            );
            return writer.CreateMessage();
        }
    }

    public Task SetDndAsync(bool state)
    {
        return this.Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = this.Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                signature: "b",
                member: "SetDnd"
            );
            writer.WriteBool(state);
            return writer.CreateMessage();
        }
    }

    public Task<bool> GetDndAsync()
    {
        return Connection.CallMethodAsync(CreateMessage(), (m, s) => ReadMessage_b(m, (NotificationsObject)s!), this);

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                member: "GetDnd"
            );
            return writer.CreateMessage();
        }
    }

    public Task CloseNotificationAsync(uint id)
    {
        return this.Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = this.Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                signature: "u",
                member: "CloseNotification"
            );
            writer.WriteUInt32(id);
            return writer.CreateMessage();
        }
    }

    public Task<bool> AddInhibitorAsync(string applicationId)
    {
        return Connection.CallMethodAsync(CreateMessage(), (m, s) => ReadMessage_b(m, (NotificationsObject)s!), this);

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                signature: "s",
                member: "AddInhibitor"
            );
            writer.WriteString(applicationId);
            return writer.CreateMessage();
        }
    }

    public Task<bool> RemoveInhibitorAsync(string applicationId)
    {
        return Connection.CallMethodAsync(CreateMessage(), (m, s) => ReadMessage_b(m, (NotificationsObject)s!), this);

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                signature: "s",
                member: "RemoveInhibitor"
            );
            writer.WriteString(applicationId);
            return writer.CreateMessage();
        }
    }

    public Task<uint> NumberOfInhibitorsAsync()
    {
        return Connection.CallMethodAsync(CreateMessage(), (m, s) => ReadMessage_u(m, (NotificationsObject)s!), this);

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                member: "NumberOfInhibitors"
            );
            return writer.CreateMessage();
        }
    }

    public Task<bool> IsInhibitedAsync()
    {
        return Connection.CallMethodAsync(CreateMessage(), (m,  s) => ReadMessage_b(m, (NotificationsObject)s!), this);

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                member: "IsInhibited"
            );
            return writer.CreateMessage();
        }
    }

    public Task<bool> ClearInhibitorsAsync()
    {
        return Connection.CallMethodAsync(CreateMessage(), (m,  s) => ReadMessage_b(m, (NotificationsObject)s!), this);

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: Interface,
                member: "ClearInhibitors"
            );
            return writer.CreateMessage();
        }
    }

    public ValueTask<IDisposable> WatchSubscribeV2Async(Action<Exception?, (uint Count, bool Dnd, bool CcOpen, bool Inhibited)> handler,
        bool emitOnCapturedContext = true, ObserverFlags flags = ObserverFlags.None)
        => WatchSignalAsync
        (
            Service.Destination, Interface, Path, "SubscribeV2", (m, s) => ReadMessage_ubbb(m, (NotificationsObject)s!), handler,
            emitOnCapturedContext, flags
        );

    public ValueTask<IDisposable> WatchSubscribeAsync(Action<Exception?, (uint Count, bool Dnd, bool CcOpen)> handler, bool emitOnCapturedContext = true,
        ObserverFlags flags = ObserverFlags.None)
        => WatchSignalAsync
        (
            Service.Destination, Interface, Path, "Subscribe", (m, s) => ReadMessage_ubb(m, (NotificationsObject)s!), handler, emitOnCapturedContext,
            flags
        );

    public Task SetInhibitedAsync(bool value)
    {
        return this.Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = this.Connection.GetMessageWriter();
            writer.WriteMethodCallHeader
            (
                destination: Service.Destination,
                path: Path,
                @interface: "org.freedesktop.DBus.Properties",
                signature: "ssv",
                member: "Set"
            );
            writer.WriteString(Interface);
            writer.WriteString("Inhibited");
            writer.WriteSignature("b");
            writer.WriteBool(value);
            return writer.CreateMessage();
        }
    }

    public Task<bool> GetInhibitedAsync()
        => Connection.CallMethodAsync
            (CreateGetPropertyMessage(Interface, "Inhibited"), (m, s) => ReadMessage_v_b(m, (NotificationsObject)s!), this);

    public Task<CcProperties> GetPropertiesAsync()
    {
        return Connection.CallMethodAsync(CreateGetAllPropertiesMessage(Interface), (m, s) => ReadMessage(m, (NotificationsObject)s!), this);

        static CcProperties ReadMessage(Message message, NotificationsObject _)
        {
            var reader = message.GetBodyReader();
            return ReadProperties(ref reader);
        }
    }

    public ValueTask<IDisposable> WatchPropertiesChangedAsync(Action<Exception?, PropertyChanges<CcProperties>> handler, bool emitOnCapturedContext = true,
        ObserverFlags flags = ObserverFlags.None)
    {
        return base.WatchPropertiesChangedAsync(Interface, (m, s) => ReadMessage(m, (NotificationsObject)s!), handler, emitOnCapturedContext, flags);

        static PropertyChanges<CcProperties> ReadMessage(Message message, NotificationsObject _)
        {
            var reader = message.GetBodyReader();
            reader.ReadString(); // interface
            List<string> changed = new(), invalidated = new();
            return new PropertyChanges<CcProperties>(ReadProperties(ref reader, changed), ReadInvalidated(ref reader), changed.ToArray());
        }

        static string[] ReadInvalidated(ref Reader reader)
        {
            List<string>? invalidated = null;
            ArrayEnd arrayEnd = reader.ReadArrayStart(DBusType.String);
            while (reader.HasNext(arrayEnd))
            {
                invalidated ??= new();
                var property = reader.ReadString();
                switch (property)
                {
                    case "Inhibited": invalidated.Add("Inhibited"); break;
                }
            }

            return invalidated?.ToArray() ?? Array.Empty<string>();
        }
    }

    private static CcProperties ReadProperties(ref Reader reader, List<string>? changedList = null)
    {
        var props = new CcProperties();
        ArrayEnd arrayEnd = reader.ReadArrayStart(DBusType.Struct);
        while (reader.HasNext(arrayEnd))
        {
            var property = reader.ReadString();
            switch (property)
            {
                case "Inhibited":
                    reader.ReadSignature("b"u8);
                    props.Inhibited = reader.ReadBool();
                    changedList?.Add("Inhibited");
                    break;
                default:
                    reader.ReadVariantValue();
                    break;
            }
        }

        return props;
    }
}

public class NotificationsService
{
    public Connection Connection { get; }
    public string Destination { get; }

    public NotificationsService(Connection connection, string destination)
        => (Connection, Destination) = (connection, destination);

    public LinuxNotificationsDbus CreateNotifications(ObjectPath path) => new LinuxNotificationsDbus(this, path);
    public Cc CreateCc(ObjectPath path) => new Cc(this, path);
}

public class NotificationsObject
{
    public NotificationsService Service { get; }
    public ObjectPath Path { get; }
    protected Connection Connection => Service.Connection;

    protected NotificationsObject(NotificationsService service, ObjectPath path)
        => (Service, Path) = (service, path);

    protected MessageBuffer CreateGetPropertyMessage(string @interface, string property)
    {
        var writer = this.Connection.GetMessageWriter();
        writer.WriteMethodCallHeader
        (
            destination: Service.Destination,
            path: Path,
            @interface: "org.freedesktop.DBus.Properties",
            signature: "ss",
            member: "Get"
        );
        writer.WriteString(@interface);
        writer.WriteString(property);
        return writer.CreateMessage();
    }

    protected MessageBuffer CreateGetAllPropertiesMessage(string @interface)
    {
        var writer = this.Connection.GetMessageWriter();
        writer.WriteMethodCallHeader
        (
            destination: Service.Destination,
            path: Path,
            @interface: "org.freedesktop.DBus.Properties",
            signature: "s",
            member: "GetAll"
        );
        writer.WriteString(@interface);
        return writer.CreateMessage();
    }

    protected ValueTask<IDisposable> WatchPropertiesChangedAsync<TProperties>(string @interface, MessageValueReader<PropertyChanges<TProperties>> reader,
        Action<Exception?, PropertyChanges<TProperties>> handler, bool emitOnCapturedContext, ObserverFlags flags)
    {
        var rule = new MatchRule
        {
            Type = MessageType.Signal,
            Sender = Service.Destination,
            Path = Path,
            Interface = "org.freedesktop.DBus.Properties",
            Member = "PropertiesChanged",
            Arg0 = @interface
        };
        return this.Connection.AddMatchAsync
        (
            rule, reader,
            (ex, changes, rs, hs) => ((Action<Exception?, PropertyChanges<TProperties>>)hs!).Invoke
                (ex, changes),
            this, handler, emitOnCapturedContext, flags
        );
    }

    public ValueTask<IDisposable> WatchSignalAsync<TArg>(string sender, string @interface, ObjectPath path, string signal, MessageValueReader<TArg> reader,
        Action<Exception?, TArg> handler, bool emitOnCapturedContext, ObserverFlags flags)
    {
        var rule = new MatchRule
        {
            Type = MessageType.Signal,
            Sender = sender,
            Path = path,
            Member = signal,
            Interface = @interface
        };
        return this.Connection.AddMatchAsync
        (
            rule, reader,
            (ex, arg, rs, hs) => ((Action<Exception?, TArg>)hs!).Invoke(ex, arg),
            this, handler, emitOnCapturedContext, flags
        );
    }

    public ValueTask<IDisposable> WatchSignalAsync(string sender, string @interface, ObjectPath path, string signal, Action<Exception?> handler,
        bool emitOnCapturedContext, ObserverFlags flags)
    {
        var rule = new MatchRule
        {
            Type = MessageType.Signal,
            Sender = sender,
            Path = path,
            Member = signal,
            Interface = @interface
        };
        return Connection.AddMatchAsync
        (
            rule, (message, state) => null!,
            (Exception? ex, object v, object? rs, object? hs) => ((Action<Exception?>)hs!).Invoke(ex), this, handler, emitOnCapturedContext, flags
        );
    }

    protected static bool ReadMessage_b(Message message, NotificationsObject _)
    {
        var reader = message.GetBodyReader();
        return reader.ReadBool();
    }

    protected static string[] ReadMessage_as(Message message, NotificationsObject _)
    {
        var reader = message.GetBodyReader();
        return reader.ReadArrayOfString();
    }

    protected static uint ReadMessage_u(Message message, NotificationsObject _)
    {
        var reader = message.GetBodyReader();
        return reader.ReadUInt32();
    }

    protected static (string, string, string, string) ReadMessage_ssss(Message message, NotificationsObject _)
    {
        var reader = message.GetBodyReader();
        var arg0 = reader.ReadString();
        var arg1 = reader.ReadString();
        var arg2 = reader.ReadString();
        var arg3 = reader.ReadString();
        return (arg0, arg1, arg2, arg3);
    }

    protected static (uint, uint) ReadMessage_uu(Message message, NotificationsObject _)
    {
        var reader = message.GetBodyReader();
        var arg0 = reader.ReadUInt32();
        var arg1 = reader.ReadUInt32();
        return (arg0, arg1);
    }

    protected static (uint, string) ReadMessage_us(Message message, NotificationsObject _)
    {
        var reader = message.GetBodyReader();
        var arg0 = reader.ReadUInt32();
        var arg1 = reader.ReadString();
        return (arg0, arg1);
    }

    protected static bool ReadMessage_v_b(Message message, NotificationsObject _)
    {
        var reader = message.GetBodyReader();
        reader.ReadSignature("b"u8);
        return reader.ReadBool();
    }

    protected static (bool, bool, uint, bool) ReadMessage_rbbubz(Message message, NotificationsObject _)
    {
        var reader = message.GetBodyReader();
        return ReadType_rbbubz(ref reader);
    }

    protected static (uint, bool, bool, bool) ReadMessage_ubbb(Message message, NotificationsObject _)
    {
        var reader = message.GetBodyReader();
        var arg0 = reader.ReadUInt32();
        var arg1 = reader.ReadBool();
        var arg2 = reader.ReadBool();
        var arg3 = reader.ReadBool();
        return (arg0, arg1, arg2, arg3);
    }

    protected static (uint, bool, bool) ReadMessage_ubb(Message message, NotificationsObject _)
    {
        var reader = message.GetBodyReader();
        var arg0 = reader.ReadUInt32();
        var arg1 = reader.ReadBool();
        var arg2 = reader.ReadBool();
        return (arg0, arg1, arg2);
    }

    protected static (bool, bool, uint, bool) ReadType_rbbubz(ref Reader reader)
    {
        return (reader.ReadBool(), reader.ReadBool(), reader.ReadUInt32(), reader.ReadBool());
    }
}

public class PropertyChanges<TProperties>
{
    public PropertyChanges(TProperties properties, string[] invalidated, string[] changed)
        => (Properties, Invalidated, Changed) = (properties, invalidated, changed);

    public TProperties Properties { get; }
    public string[] Invalidated { get; }
    public string[] Changed { get; }
    public bool HasChanged(string property) => Array.IndexOf(Changed, property) != -1;
    public bool IsInvalidated(string property) => Array.IndexOf(Invalidated, property) != -1;
}
