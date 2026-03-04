using System.Buffers;
using System.Buffers.Binary;

using Moq;

using Tiempito.IPC.Abstractions;
using Tiempito.IPC.Implementations;
using Tiempito.IPC.Models;
using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.IPC.UnitTests.Implementations;

/// <summary>
/// Unit tests for the <see cref="MessageHandler"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "IPC")]
public sealed class MessageHandlerUnitTests : IDisposable
{
    private const int Int32SizeBytes = 4;

    private readonly MockRepository _mockRepository;
    private readonly Mock<ArrayPool<byte>> _poolMock;
    private readonly Mock<IMessageSerializer> _serializerMock;
    private readonly Mock<Stream> _streamMock;
    private readonly MessageHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageHandlerUnitTests"/> class.
    /// </summary>
    public MessageHandlerUnitTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Loose);
        _poolMock = _mockRepository.Create<ArrayPool<byte>>();
        _serializerMock = _mockRepository.Create<IMessageSerializer>();
        _streamMock = _mockRepository.Create<Stream>();
        _handler = new MessageHandler(_poolMock.Object, _serializerMock.Object);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Tests that <see cref="MessageHandler.WriteAsync"/> writes the expected message to the stream.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task WriteAsync_should_WriteExpectedMessage()
    {
        // Arrange
        var message = PauseSessionCommand.CreateNew();
        byte[] lengthBuffer = new byte[Int32SizeBytes];
        byte[] msgBytes = new byte[12];
        _streamMock.Setup(m => m.CanWrite).Returns(true);
        _poolMock.Setup(m => m.Rent(Int32SizeBytes)).Returns(lengthBuffer);
        _serializerMock.Setup(m => m.Serialize(message)).Returns(msgBytes);

        // Act
        await _handler.WriteAsync(_streamMock.Object, message);

        // Assert
        _streamMock.Verify(m => m.WriteAsync(lengthBuffer), Times.Once);
        _streamMock.Verify(m => m.WriteAsync(msgBytes), Times.Once);
        _streamMock.Verify(m => m.FlushAsync(It.IsAny<CancellationToken>()), Times.Once);
        _poolMock.Verify(m => m.Return(lengthBuffer), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="MessageHandler.WriteAsync"/> throws <see cref="ArgumentNullException"/> when the given stream is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task WriteAsync_should_ThrowArgumentNullException_when_GivenStreamIsNull()
    {
        // Arrange
        var message = PauseSessionCommand.CreateNew();
        Stream? stream = null;

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.WriteAsync(stream!, message));
    }

    /// <summary>
    /// Tests that <see cref="MessageHandler.WriteAsync"/> throws <see cref="ArgumentNullException"/> when the given message is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task WriteAsync_should_ThrowArgumentNullException_when_GivenMessageIsNull()
    {
        // Arrange
        Message? message = null;
        await using Stream stream = new MemoryStream();

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.WriteAsync(stream, message));
    }

    /// <summary>
    /// Tests that <see cref="MessageHandler.WriteAsync"/> throws <see cref="NotSupportedException"/> when the given stream is not writable.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task WriteAsync_should_ThrowNotSupportedException_when_GivenStreamIsNotWritable()
    {
        // Arrange
        var message = PauseSessionCommand.CreateNew();
        _streamMock.Setup(m => m.CanWrite).Returns(false);

        // Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => _handler.WriteAsync(_streamMock.Object, message));
    }

    /// <summary>
    /// Tests that <see cref="MessageHandler.ReadAsync{T}"/> returns the expected message when reading from the stream.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ReadAsync_should_ReturnExpectedMessage()
    {
        // Arrange
        var expected = PauseSessionCommand.CreateNew();
        byte[] lengthBuffer = new byte[Int32SizeBytes];
        byte[] msgBytes = new byte[12];
        BinaryPrimitives.WriteInt32LittleEndian(lengthBuffer, msgBytes.Length);
        _streamMock.Setup(m => m.CanRead).Returns(true);
        _streamMock.Setup(m => m.ReadAsync(lengthBuffer, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lengthBuffer.Length);
        _streamMock.Setup(m => m.ReadAsync(msgBytes, It.IsAny<CancellationToken>())).ReturnsAsync(msgBytes.Length);
        _poolMock.Setup(m => m.Rent(Int32SizeBytes)).Returns(lengthBuffer);
        _poolMock.Setup(m => m.Rent(msgBytes.Length)).Returns(msgBytes);
        _serializerMock.Setup(m => m.Deserialize<PauseSessionCommand>(msgBytes)).Returns(expected);

        // Act
        PauseSessionCommand? actual = await _handler.ReadAsync<PauseSessionCommand>(_streamMock.Object);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
        _poolMock.Verify(m => m.Return(lengthBuffer), Times.Once);
        _poolMock.Verify(m => m.Return(msgBytes), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="MessageHandler.ReadAsync{T}"/> throws <see cref="ArgumentNullException"/> when the given stream is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ReadAsync_should_ThrowArgumentNullException_when_GivenStreamIsNull()
    {
        // Arrange
        Stream? stream = null;

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.ReadAsync<PauseSessionCommand>(stream!));
    }

    /// <summary>
    /// Tests that <see cref="MessageHandler.ReadAsync{T}"/> throws <see cref="NotSupportedException"/> when the given stream is not readable.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ReadAsync_should_ThrowNotSupportedException_when_GivenStreamIsNotReadable()
    {
        // Arrange
        _streamMock.Setup(m => m.CanRead).Returns(false);

        // Assert
        await Assert.ThrowsAsync<NotSupportedException>(
            () => _handler.ReadAsync<PauseSessionCommand>(_streamMock.Object));
    }

    /// <summary>
    /// Tests that <see cref="MessageHandler.ReadAsync{T}"/> returns null when the stream reaches the end.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ReadAsync_should_ReturnNull_when_StreamReachesEnd()
    {
        // Arrange
        byte[] lengthBuffer = new byte[Int32SizeBytes];
        _streamMock.Setup(m => m.CanRead).Returns(true);
        _streamMock.Setup(m => m.ReadAsync(lengthBuffer, It.IsAny<CancellationToken>())).Throws<EndOfStreamException>();
        _poolMock.Setup(m => m.Rent(Int32SizeBytes)).Returns(lengthBuffer);

        // Act
        PauseSessionCommand? actual = await _handler.ReadAsync<PauseSessionCommand>(_streamMock.Object);

        // Assert
        Assert.Null(actual);
    }
}