using System.CommandLine;

using Moq;

using Tiempito.CLI.Commands.Config;
using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Models;

using IpcCommand = Tiempito.IPC.Models.Commands.Config.CreateSessionConfigCommand;

namespace Tiempito.CLI.UnitTests.Commands.Config;

/// <summary>
/// Unit tests for the <see cref="CreateSessionConfigCommand"/>.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature ", "CLI")]
public sealed class CreateSessionConfigCommandTests : IDisposable
{
    private readonly MockRepository _mockRepository;
    private readonly Mock<ICommandSender> _commandSenderMock;
    private readonly Mock<IMessageWriter> _messageWriterMock;
    private readonly RootCommand _rootCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateSessionConfigCommandTests"/> class.
    /// </summary>
    public CreateSessionConfigCommandTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Loose);
        _commandSenderMock = _mockRepository.Create<ICommandSender>();
        _messageWriterMock = _mockRepository.Create<IMessageWriter>();
        _rootCommand = new RootCommand("Tiempito CLI")
        {
            new ConfigCommand(_commandSenderMock.Object, _messageWriterMock.Object),
        };
    }

    /// <summary>
    /// Provides test cases for the <see cref="It_should_ExecuteCommandCorrectly"/> theory.
    /// </summary>
    /// <returns>A <see cref="TheoryData"/> containing the test inputs and expected responses.</returns>
    public static TheoryData<string, uint, string, string, string, Response> GetTestCases()
    {
        return new TheoryData<string, uint, string, string, string, Response>
        {
            { "1", 2, "5h", "3m", "60s", Response.Ok(Guid.Empty, "Test message") },
            { "daily-focus", 4, "25m", "5m", "0s", Response.BadRequest(Guid.Empty, "Updated config") },
        };
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Verifies that the create-session configuration command executes correctly with various inputs.
    /// </summary>
    /// <param name="configId">The identifier for the session configuration.</param>
    /// <param name="targetCycles">The number of target cycles.</param>
    /// <param name="focusDuration">The duration of focus periods.</param>
    /// <param name="breakDuration">The duration of break periods.</param>
    /// <param name="delayBetweenTimes">The delay between sessions.</param>
    /// <param name="response">The expected response from the command sender.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [MemberData(nameof(GetTestCases))]
    public async Task It_should_ExecuteCommandCorrectly(
        string configId,
        uint targetCycles,
        string focusDuration,
        string breakDuration,
        string delayBetweenTimes,
        Response response)
    {
        // Arrange
        var expectedCmd = IpcCommand.CreateNew(configId, targetCycles, focusDuration, breakDuration, delayBetweenTimes);
        string cmd = $"config create-session -ci {configId} -t {targetCycles} -f {focusDuration} -b {breakDuration} -d {delayBetweenTimes}";
        var parseResult = _rootCommand.Parse(cmd);
        _commandSenderMock.Setup(m => m.SendAsync(It.IsAny<IpcCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await parseResult.InvokeAsync();

        // Assert
        _commandSenderMock.Verify(
            m => m.SendAsync(
                It.Is<IpcCommand>(c => c.SessionConfigId == expectedCmd.SessionConfigId
                    && c.TargetCycles == expectedCmd.TargetCycles && c.FocusDuration == expectedCmd.FocusDuration
                    && c.BreakDuration == expectedCmd.BreakDuration
                    && c.DelayBetweenTimes == expectedCmd.DelayBetweenTimes), It.IsAny<CancellationToken>()),
            Times.Once);
        _messageWriterMock.Verify(
            m => m.WriteLineAsync(!response.Success, response.Message, It.IsAny<CancellationToken>()));
    }

    /// <summary>
    /// Verifies that an error message is displayed when the provided target cycles value is not a valid number.
    /// </summary>
    /// <param name="targetCycles">The invalid target cycles input string.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("-1")]
    [InlineData("n")]
    [InlineData("$")]
    public async Task It_should_DisplayErrorMessage_when_GivenTargetCyclesIsNotAValidNumber(string targetCycles)
    {
        // Arrange
        string cmd = $"config create-session -ci 1 -t {targetCycles} -f 25m -b 5m -d 60s";
        var parseResult = _rootCommand.Parse(cmd);

        // Act
        await parseResult.InvokeAsync();

        // Assert
        _messageWriterMock.Verify(m => m.WriteLineAsync(true, It.IsAny<string>(), It.IsAny<CancellationToken>()));
    }
}