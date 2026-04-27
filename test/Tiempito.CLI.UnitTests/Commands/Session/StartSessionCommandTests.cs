using System.CommandLine;

using Moq;

using Tiempito.CLI.Commands.Session;
using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Models;

using IpcStartCommand = Tiempito.IPC.Models.Commands.Session.StartSessionCommand;

namespace Tiempito.CLI.UnitTests.Commands.Session;

/// <summary>
/// Unit tests for the <see cref="StartSessionCommand"/>.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature ", "CLI")]
public sealed class StartSessionCommandTests : IDisposable
{
    private readonly MockRepository _mockRepository;
    private readonly Mock<ICommandSender> _commandSenderMock;
    private readonly Mock<IMessageWriter> _messageWriterMock;
    private readonly Mock<ISessionFollower> _sessionFollowerMock;
    private readonly RootCommand _rootCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartSessionCommandTests"/> class.
    /// </summary>
    public StartSessionCommandTests()
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

    /// <summary>
    /// Gets the test cases for the <see cref="It_should_ExecuteCommandCorrectly"/> test.
    /// </summary>
    /// <returns>A <see cref="TheoryData{T1, T2, T3, T4}"/> containing session ID, session configuration ID, follow flag, and expected response.</returns>
    public static TheoryData<string, string, bool, Response> GetTestCases()
    {
        var guid = Guid.NewGuid();
        return new TheoryData<string, string, bool, Response>
        {
            { "testId", "testConfigId", true, Response.Ok(guid, "Test message") },
            { "testId", "testConfigId", false, Response.Ok(guid, "Test message") },
            { "testId", "testConfigId", true, Response.BadRequest(guid, "Test message") },
            { "testId", "testConfigId", false, Response.BadRequest(guid, "Test message") },
        };
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Verifies that the command executes correctly with various input parameters.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="sessionConfigId">The session configuration identifier.</param>
    /// <param name="follow">A value indicating whether the session should be followed.</param>
    /// <param name="response">The expected response from the command sender.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [MemberData(nameof(GetTestCases))]
    public async Task It_should_ExecuteCommandCorrectly(
        string sessionId,
        string sessionConfigId,
        bool follow,
        Response response)
    {
        // Arrange
        var followFlag = follow ? "-f" : string.Empty;
        var parseResult = _rootCommand.Parse($"session start -i {sessionId} -ci {sessionConfigId} {followFlag}");
        var followSetVerifyTimes = follow && response.Success ? Times.Once() : Times.Never();
        _commandSenderMock.Setup(m => m.SendAsync(It.IsAny<IpcStartCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await parseResult.InvokeAsync();

        // Assert
        _commandSenderMock.Verify(
            m => m.SendAsync(
                It.Is<IpcStartCommand>(c => c.SessionId == sessionId && c.SessionConfigId == sessionConfigId),
                It.IsAny<CancellationToken>()), Times.Once);
        _messageWriterMock.Verify(
            m => m.WriteLineAsync(error: !response.Success, response.Message, It.IsAny<CancellationToken>()), Times.Once);
        _sessionFollowerMock.VerifySet(m => m.MustFollow = true, followSetVerifyTimes);
    }

    /// <summary>
    /// Verifies that the command provides an empty session ID when the identity argument is not provided.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task It_should_ProvideCommandWithEmptySessionId_when_IdArgIsNotProvided()
    {
        // Arrange
        var parseResult = _rootCommand.Parse("session start -ci testConfigId");

        // Act
        await parseResult.InvokeAsync();

        // Assert
        _commandSenderMock.Verify(
            m => m.SendAsync(It.Is<IpcStartCommand>(c => c.SessionId == string.Empty), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that the command provides an empty session configuration ID when the configuration identity argument is not provided.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task It_should_ProvideCommandWithEmptySessionConfigId_when_ConfigIdArgIsNotProvided()
    {
        // Arrange
        var parseResult = _rootCommand.Parse("session start -i testId");

        // Act
        await parseResult.InvokeAsync();

        // Assert
        _commandSenderMock.Verify(
            m => m.SendAsync(
                It.Is<IpcStartCommand>(c => c.SessionConfigId == string.Empty), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}