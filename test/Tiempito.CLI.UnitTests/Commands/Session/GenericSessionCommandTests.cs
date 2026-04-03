using System.CommandLine;

using Moq;

using Tiempito.CLI.Commands.Session;
using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Models;
using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.CLI.UnitTests.Commands.Session;

/// <summary>
/// Unit tests for the <see cref="GenericSessionCommand{TCommand}"/>.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature ", "CLI")]
public sealed class GenericSessionCommandTests : IDisposable
{
    private readonly MockRepository _mockRepository;
    private readonly Mock<ICommandSender> _commandSenderMock;
    private readonly Mock<IMessageWriter> _messageWriterMock;
    private readonly Mock<ISessionFollower> _sessionFollowerMock;
    private readonly RootCommand _rootCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericSessionCommandTests"/> class.
    /// </summary>
    public GenericSessionCommandTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Loose);
        _commandSenderMock = _mockRepository.Create<ICommandSender>();
        _messageWriterMock = _mockRepository.Create<IMessageWriter>();
        _sessionFollowerMock = _mockRepository.Create<ISessionFollower>();
        _rootCommand = new RootCommand("Tiempito CLI")
        {
            new SessionCommand(_commandSenderMock.Object, _messageWriterMock.Object, _sessionFollowerMock.Object),
        };
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Verifies that the cancel session command is executed correctly when the command type is <see cref="CancelSessionCommand"/>.
    /// </summary>
    /// <param name="cmd">The command line string to parse.</param>
    /// <param name="sessionId">The expected session identifier.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("session cancel", "")]
    [InlineData("session cancel -i testId", "testId")]
    [InlineData("session cancel --id testId", "testId")]
    public async Task It_should_ExecuteCancelSessionCommand_when_TCommandIsCancel(string cmd, string sessionId)
    {
        // Arrange
        var response = Response.Ok(Guid.Empty, "Test message");
        var parseResult = _rootCommand.Parse(cmd);
        _commandSenderMock.Setup(m => m.SendAsync(It.IsAny<CancelSessionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await parseResult.InvokeAsync();

        // Assert
        _commandSenderMock.Verify(
            m => m.SendAsync(It.Is<CancelSessionCommand>(c => c.SessionId == sessionId), It.IsAny<CancellationToken>()),
            Times.Once);
        _messageWriterMock.Verify(
            m => m.WriteLineAsync(!response.Success, response.Message, It.IsAny<CancellationToken>()),
            Times.Once);
        _sessionFollowerMock.VerifySet(m => m.MustFollow = true, Times.Never);
    }

    /// <summary>
    /// Verifies that the pause session command is executed correctly when the command type is <see cref="PauseSessionCommand"/>.
    /// </summary>
    /// <param name="cmd">The command line string to parse.</param>
    /// <param name="sessionId">The expected session identifier.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("session pause", "")]
    [InlineData("session pause -i testId", "testId")]
    [InlineData("session pause --id testId", "testId")]
    public async Task It_should_ExecutePauseSessionCommand_when_TCommandIsPause(string cmd, string sessionId)
    {
        // Arrange
        var response = Response.Ok(Guid.Empty, "Test message");
        var parseResult = _rootCommand.Parse(cmd);
        _commandSenderMock.Setup(m => m.SendAsync(It.IsAny<PauseSessionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await parseResult.InvokeAsync();

        // Assert
        _commandSenderMock.Verify(
            m => m.SendAsync(It.Is<PauseSessionCommand>(c => c.SessionId == sessionId), It.IsAny<CancellationToken>()),
            Times.Once);
        _messageWriterMock.Verify(
            m => m.WriteLineAsync(!response.Success, response.Message, It.IsAny<CancellationToken>()),
            Times.Once);
        _sessionFollowerMock.VerifySet(m => m.MustFollow = true, Times.Never);
    }

    /// <summary>
    /// Verifies that the resume session command is executed correctly when the command type is <see cref="ResumeSessionCommand"/>.
    /// </summary>
    /// <param name="cmd">The command line string to parse.</param>
    /// <param name="sessionId">The expected session identifier.</param>
    /// <param name="follow">If true, the CLI should follow the session after resuming.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("session resume", "", false)]
    [InlineData("session resume -i testId", "testId", false)]
    [InlineData("session resume --id testId", "testId", false)]
    [InlineData("session resume -f", "", true)]
    [InlineData("session resume --follow", "", true)]
    [InlineData("session resume -i testId -f", "testId", true)]
    [InlineData("session resume -i testId --follow", "testId", true)]
    [InlineData("session resume --id testId -f", "testId", true)]
    [InlineData("session resume --id testId --follow", "testId", true)]
    public async Task It_should_ExecuteCancelSessionCommand_when_TCommandIsResume(
        string cmd,
        string sessionId,
        bool follow)
    {
        // Arrange
        var response = Response.Ok(Guid.Empty, "Test message");
        var parseResult = _rootCommand.Parse(cmd);
        var followSetVerifyTimes = follow && response.Success ? Times.Once() : Times.Never();
        _commandSenderMock.Setup(m => m.SendAsync(It.IsAny<ResumeSessionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await parseResult.InvokeAsync();

        // Assert
        _commandSenderMock.Verify(
            m => m.SendAsync(It.Is<ResumeSessionCommand>(c => c.SessionId == sessionId), It.IsAny<CancellationToken>()),
            Times.Once);
        _messageWriterMock.Verify(
            m => m.WriteLineAsync(!response.Success, response.Message, It.IsAny<CancellationToken>()),
            Times.Once);
        _sessionFollowerMock.VerifySet(m => m.MustFollow = true, followSetVerifyTimes);
    }
}