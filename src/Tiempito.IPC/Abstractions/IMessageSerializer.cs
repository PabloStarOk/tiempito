namespace Tiempito.IPC.Abstractions;

/// <summary>
/// Defines methods for serializing and deserializing messages.
/// </summary>
internal interface IMessageSerializer
{
    /// <summary>
    /// Serializes a message of type <typeparamref name="TMessage"/> to a byte array.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to serialize.</typeparam>
    /// <param name="message">The message instance to serialize. Must not be null.</param>
    /// <returns>A byte array representing the serialized message.</returns>
    byte[] Serialize<TMessage>(TMessage message)
        where TMessage : notnull;

    /// <summary>
    /// Deserializes a byte array to a message of type <typeparamref name="TMessage"/>.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to deserialize.</typeparam>
    /// <param name="data">The byte array containing the serialized message data.</param>
    /// <returns>The deserialized message instance.</returns>
    TMessage Deserialize<TMessage>(ReadOnlyMemory<byte> data)
        where TMessage : notnull;
}
