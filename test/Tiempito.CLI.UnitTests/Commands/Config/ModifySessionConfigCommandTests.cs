using System.CommandLine;

using Moq;

using Tiempito.CLI.Commands.Config;
using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Models;

using IpcCommand = Tiempito.IPC.Models.Commands.Config.ModifySessionConfigCommand;

namespace Tiempito.CLI.UnitTests.Commands.Config;

/// <summary>
/// Unit tests for the <see cref="ModifySessionConfigCommand"/>.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature ", "CLI")]
public sealed class ModifySessionConfigCommandTests : IDisposable
{
    private readonly MockRepository _mockRepository;
    private readonly Mock<ICommandSender> _commandSenderMock;
    private readonly Mock<IMessageWriter> _messageWriterMock;
    private readonly RootCommand _rootCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModifySessionConfigCommandTests"/> class.
    /// </summary>
    public ModifySessionConfigCommandTests()
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
    /// Provides test cases for the <see cref="It_should_ExecuteCommandCorrectly"/> test method.
    /// </summary>
    /// <returns>A collection of test case data.</returns>
    public static TheoryData<string, uint?, string?, string?, string?, Response> GetTestCases()
    {
        return new TheoryData<string, uint?, string?, string?, string?, Response>
        {
            { "daily-focus", 2, "5h", "3m", "60s", Response.Ok(Guid.Empty, "Test message") },
            { "daily-focus", null, "5h", "3m", "60s", Response.Ok(Guid.Empty, "Test message") },
            { "daily-focus", 2, null, "3m", "60s", Response.Ok(Guid.Empty, "Test message") },
            { "daily-focus", 2, "5h", null, "60s", Response.Ok(Guid.Empty, "Test message") },
            { "daily-focus", 2, "5h", "3m", null, Response.Ok(Guid.Empty, "Test message") },
            { "daily-focus", null, "5h", null, null, Response.Ok(Guid.Empty, "Test message") },
            { "daily-focus", 4, "25m", "5m", "0s", Response.BadRequest(Guid.Empty, "Updated config") },
            { "daily-focus", null, "25m", "5m", "0s", Response.BadRequest(Guid.Empty, "Updated config") },
            { "daily-focus", 4, null, "5m", "0s", Response.BadRequest(Guid.Empty, "Updated config") },
            { "daily-focus", 4, "25m", null, "0s", Response.BadRequest(Guid.Empty, "Updated config") },
            { "daily-focus", 4, "25m", "5m", null, Response.BadRequest(Guid.Empty, "Updated config") },
            { "daily-focus", null, null, "5m", null, Response.BadRequest(Guid.Empty, "Updated config") },
        };
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Verifies that the modify-session command executes correctly and calls the command sender with the expected parameters.
    /// </summary>
    /// <param name="configId">The session configuration identifier.</param>
    /// <param name="targetCycles">The number of target cycles.</param>
    /// <param name="focusDuration">The focus duration string.</param>
    /// <param name="breakDuration">The break duration string.</param>
    /// <param name="delayBetweenTimes">The delay between times string.</param>
    /// <param name="response">The expected response from the command sender.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [MemberData(nameof(GetTestCases))]
    public async Task It_should_ExecuteCommandCorrectly(
        string configId,
        uint? targetCycles,
        string? focusDuration,
        string? breakDuration,
        string? delayBetweenTimes,
        Response response)
    {
        // Arrange
        var expectedCmd = IpcCommand.CreateNew(configId, targetCycles, focusDuration, breakDuration, delayBetweenTimes);
        string? targetCyclesArg = targetCycles is not null ? $"-t {targetCycles}" : null;
        string? focusDurationArg = focusDuration is not null ? $"-f {focusDuration}" : null;
        string? breakDurationArg = breakDuration is not null ? $"-b {breakDuration}" : null;
        string? delayArg = delayBetweenTimes is not null ? $"-d {delayBetweenTimes}" : null;
        string cmd = $"config modify-session -ci {configId} {targetCyclesArg} {focusDurationArg} {breakDurationArg} {delayArg}";
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
    /// Verifies that an error message is displayed when an invalid target cycles value is provided.
    /// </summary>
    /// <param name="targetCycles">The invalid target cycles input.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("-1")]
    [InlineData("n")]
    [InlineData("$")]
    public async Task It_should_DisplayErrorMessage_when_GivenTargetCyclesIsNotAValidNumber(string targetCycles)
    {
        // Arrange
        string cmd = $"config modify-session -ci 1 -t {targetCycles}";
        var parseResult = _rootCommand.Parse(cmd);

        // Act
        await parseResult.InvokeAsync();

        // Assert
        _messageWriterMock.Verify(m => m.WriteLineAsync(true, It.IsAny<string>(), It.IsAny<CancellationToken>()));
    }
}