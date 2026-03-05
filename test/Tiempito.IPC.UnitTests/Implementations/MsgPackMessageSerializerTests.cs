using MessagePack;

using Tiempito.IPC.Implementations;
using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.IPC.UnitTests.Implementations;

/// <summary>
/// Unit tests for the <see cref="MsgPackMessageSerializer"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "IPC")]
public sealed class MsgPackMessageSerializerTests
{
    private static readonly MessagePackSerializerOptions MsgPackOptions = MessagePackSerializerOptions.Standard;
    private readonly MsgPackMessageSerializer _serializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MsgPackMessageSerializerTests"/> class.
    /// </summary>
    public MsgPackMessageSerializerTests()
    {
        _serializer = new MsgPackMessageSerializer(MsgPackOptions);
    }

    /// <summary>
    /// Tests that <see cref="MsgPackMessageSerializer.Serialize"/> returns the expected byte array for a given model.
    /// </summary>
    [Fact]
    public void Serialize_should_ReturnExpectedByteArray()
    {
        // Arrange
        var startCmd = StartSessionCommand.CreateNew(sessionId: "test", sessionConfigId: "test");
        byte[] expected = MessagePackSerializer.Serialize(startCmd, MsgPackOptions);

        // Act
        byte[] actual = _serializer.Serialize(startCmd);

        // Assert
        Assert.Equivalent(expected, actual);
    }

    /// <summary>
    /// Tests that <see cref="MsgPackMessageSerializer.Deserialize{T}"/> correctly deserializes a byte array into the expected model.
    /// </summary>
    [Fact]
    public void Deserialize_should_ReturnExpectedModel()
    {
        // Arrange
        var expected = StartSessionCommand.CreateNew(sessionId: "test", sessionConfigId: "test");
        byte[] bytes = MessagePackSerializer.Serialize(expected, MsgPackOptions);

        // Act
        var actual = _serializer.Deserialize<StartSessionCommand>(bytes);

        // Assert
        Assert.Equal(expected, actual);
    }
}