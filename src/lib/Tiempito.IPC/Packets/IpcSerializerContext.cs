using System.Text.Json.Serialization;

using Tiempito.IPC.Messages;

namespace Tiempito.IPC.Packets;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Packet))]
[JsonSerializable(typeof(Request))]
[JsonSerializable(typeof(Response))]
public partial class IpcSerializerContext : JsonSerializerContext;
