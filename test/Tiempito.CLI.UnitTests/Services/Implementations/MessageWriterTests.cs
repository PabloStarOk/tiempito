using Moq;

using Tiempito.CLI.Services.Implementations;

namespace Tiempito.CLI.UnitTests.Services.Implementations;

/// <summary>
/// Unit tests for the <see cref="MessageWriter"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature ", "CLI")]
public sealed class MessageWriterTests : IDisposable
{
    private static readonly CancellationToken CancellationToken = CancellationToken.None;
    private readonly MockRepository _mockRepository;
    private readonly Mock<TextWriter> _stdOutMock;
    private readonly Mock<TextWriter> _stdErrMock;
    private readonly MessageWriter _messageWriter;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageWriterTests"/> class.
    /// </summary>
    public MessageWriterTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Loose);
        _stdOutMock = _mockRepository.Create<TextWriter>();
        _stdErrMock = _mockRepository.Create<TextWriter>();
        _messageWriter = new MessageWriter(_stdOutMock.Object, _stdErrMock.Object);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Tests that <see cref="MessageWriter.WriteLineAsync"/> writes the message to standard output when the error parameter is false.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task WriteLineAsync_should_WriteMessageToStdOut_when_ErrorParamIsFalse()
    {
        // Arrange
        const string msg = "test";

        // Act
        await _messageWriter.WriteLineAsync(error: false, msg, CancellationToken);

        // Assert
        _stdOutMock.Verify(s => s.WriteLineAsync(msg.AsMemory(), CancellationToken), Times.Once);
        _stdErrMock.Verify(s => s.WriteLineAsync(msg.AsMemory(), CancellationToken), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="MessageWriter.WriteLineAsync"/> writes the message to standard error when the error parameter is true.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task WriteLineAsync_should_WriteMessageToStdErr_when_ErrorParamIsTrue()
    {
        // Arrange
        const string msg = "test";

        // Act
        await _messageWriter.WriteLineAsync(error: true, msg, CancellationToken);

        // Assert
        _stdErrMock.Verify(s => s.WriteLineAsync(msg.AsMemory(), CancellationToken), Times.Once);
        _stdOutMock.Verify(s => s.WriteLineAsync(msg.AsMemory(), CancellationToken), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="MessageWriter.WriteAsync"/> writes the message to standard output when the error parameter is false.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task WriteAsync_should_WriteMessageToStdOut_when_ErrorParamIsFalse()
    {
        // Arrange
        const string msg = "test";

        // Act
        await _messageWriter.WriteAsync(error: false, msg, CancellationToken);

        // Assert
        _stdOutMock.Verify(s => s.WriteAsync(msg.AsMemory(), CancellationToken), Times.Once);
        _stdErrMock.Verify(s => s.WriteAsync(msg.AsMemory(), CancellationToken), Times.Never);
    }

    /// <summary>
    /// Tests that <see cref="MessageWriter.WriteAsync"/> writes the message to standard error when the error parameter is true.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task WriteAsync_should_WriteMessageToStdErr_when_ErrorParamIsTrue()
    {
        // Arrange
        const string msg = "test";

        // Act
        await _messageWriter.WriteAsync(error: true, msg, CancellationToken);

        // Assert
        _stdOutMock.Verify(s => s.WriteAsync(msg.AsMemory(), CancellationToken), Times.Never);
        _stdErrMock.Verify(s => s.WriteAsync(msg.AsMemory(), CancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="MessageWriter.ClearLineAsync"/> clears the current line in the output.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ClearLineAsync_should_ClearTheLine()
    {
        // Arrange
        const string eraseAnsiString = "\r\x1B[2K";

        // Act
        await _messageWriter.ClearLineAsync(CancellationToken);

        // Assert
        _stdOutMock.Verify(s => s.WriteAsync(eraseAnsiString.AsMemory(), CancellationToken), Times.Once);
    }
}