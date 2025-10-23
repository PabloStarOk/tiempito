using System.Buffers;
using System.Buffers.Binary;

using Tiempito.IPC.Abstractions;

namespace Tiempito.IPC.Implementations;

/// <summary>
/// Handles message serialization and deserialization, providing methods to write and read messages to and from streams.
/// Implements <see cref="IMessageWriter"/> and <see cref="IMessageReader"/>.
/// </summary>
internal sealed class MessageHandler : IMessageWriter, IMessageReader
{
    private const int LengthPrefixBufferSize = 4;
    private readonly ArrayPool<byte> _byteArrayPool;
    private readonly IMessageSerializer _messageSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageHandler"/> class.
    /// </summary>
    /// <param name="byteArrayPool">The array pool used for renting and returning byte buffers.</param>
    /// <param name="messageSerializer">The serializer used for message serialization and deserialization.</param>
    public MessageHandler(ArrayPool<byte> byteArrayPool, IMessageSerializer messageSerializer)
    {
        _byteArrayPool = byteArrayPool;
        _messageSerializer = messageSerializer;
    }

    /// <inheritdoc/>
    public async Task WriteAsync<TMessage>(
        Stream stream,
        TMessage message,
        CancellationToken cancellationToken = default)
        where TMessage : notnull
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(message);

        if (!stream.CanWrite)
        {
            throw new NotSupportedException("Stream does not support write operations.");
        }

        byte[] lengthPrefixBuffer = _byteArrayPool.Rent(LengthPrefixBufferSize);

        try
        {
            byte[] serializedMsg = _messageSerializer.Serialize(message);
            BinaryPrimitives.WriteInt32LittleEndian(lengthPrefixBuffer, serializedMsg.Length);
            await stream.WriteAsync(lengthPrefixBuffer, cancellationToken);
            await stream.WriteAsync(serializedMsg, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
        finally
        {
            _byteArrayPool.Return(lengthPrefixBuffer);
        }
    }

    /// <inheritdoc/>
    public async Task<TMessage?> ReadAsync<TMessage>(
        Stream stream,
        CancellationToken cancellationToken = default)
        where TMessage : notnull
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
        {
            throw new NotSupportedException("Stream does not support read operations.");
        }

        byte[] lengthPrefixBuffer = _byteArrayPool.Rent(LengthPrefixBufferSize);

        try
        {
            await stream.ReadExactlyAsync(lengthPrefixBuffer, cancellationToken);
            int msgLength = BinaryPrimitives.ReadInt32LittleEndian(lengthPrefixBuffer);
            return await ReadMessageAsync<TMessage>(stream, msgLength, cancellationToken);
        }
        catch (EndOfStreamException)
        {
            return default;
        }
        finally
        {
            _byteArrayPool.Return(lengthPrefixBuffer);
        }
    }

    private async Task<TMessage> ReadMessageAsync<TMessage>(
        Stream stream,
        int msgLength,
        CancellationToken cancellationToken)
        where TMessage : notnull
    {
        byte[] serializedMsgBuffer = _byteArrayPool.Rent(msgLength);

        try
        {
            await stream.ReadExactlyAsync(serializedMsgBuffer.AsMemory(0, msgLength), cancellationToken);
            return _messageSerializer.Deserialize<TMessage>(serializedMsgBuffer);
        }
        finally
        {
            _byteArrayPool.Return(serializedMsgBuffer);
        }
    }
}
