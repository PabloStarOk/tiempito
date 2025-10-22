using MessagePack;

using Tiempito.IPC.Abstractions;

namespace Tiempito.IPC.Implementations;

/// <summary>
/// Provides serialization and deserialization of messages using MessagePack.
/// </summary>
internal sealed class MsgPackMessageSerializer : IMessageSerializer
{
    private readonly MessagePackSerializerOptions _serializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="MsgPackMessageSerializer"/> class with the specified MessagePack serializer options.
    /// </summary>
    /// <param name="serializerOptions">The options to use for MessagePack serialization and deserialization.</param>
    public MsgPackMessageSerializer(MessagePackSerializerOptions serializerOptions)
    {
        _serializerOptions = serializerOptions;
    }

    /// <inheritdoc/>
    public byte[] Serialize<TModel>(TModel message)
        where TModel : notnull
    {
        return MessagePackSerializer.Serialize(message, _serializerOptions, CancellationToken.None);
    }

    /// <inheritdoc/>
    public TModel Deserialize<TModel>(ReadOnlyMemory<byte> data)
        where TModel : notnull
    {
        return MessagePackSerializer.Deserialize<TModel>(data, _serializerOptions);
    }
}
