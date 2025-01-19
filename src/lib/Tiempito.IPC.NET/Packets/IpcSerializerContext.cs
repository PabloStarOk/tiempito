using System.Text.Json.Serialization;
using Tiempito.IPC.NET.Messages;

namespace Tiempito.IPC.NET.Packets;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Packet))]
[JsonSerializable(typeof(Request))]
[JsonSerializable(typeof(Response))]
public partial class IpcSerializerContext : JsonSerializerContext;
