using System.CommandLine;

using Moq;

using Tiempito.CLI.Commands.Config;
using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Models;

using IpcCommand = Tiempito.IPC.Models.Commands.Config.SetConfigCommand;

namespace Tiempito.CLI.UnitTests.Commands.Config;

/// <summary>
/// Unit tests for the <see cref="SetConfigCommand"/>.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature ", "CLI")]
public sealed class SetConfigCommandTests : IDisposable
{
    private readonly MockRepository _mockRepository;
    private readonly Mock<ICommandSender> _commandSenderMock;
    private readonly Mock<IMessageWriter> _messageWriterMock;
    private readonly RootCommand _rootCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetConfigCommandTests"/> class.
    /// </summary>
    public SetConfigCommandTests()
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
    public static TheoryData<string, string, Response> GetTestCases()
    {
        return new TheoryData<string, string, Response>
        {
            { "config set -d daily-focus", "daily-focus", Response.Ok(Guid.Empty, "Test message") },
            { "config set --default-config daily-focus", "daily-focus", Response.Ok(Guid.Empty, "Test message") },
            { "config set -d daily-focus", "daily-focus", Response.BadRequest(Guid.Empty, "Test message") },
        };
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Verifies that the set config command executes correctly with various inputs and responses.
    /// </summary>
    /// <param name="cmd">The command string to parse and execute.</param>
    /// <param name="defaultConfigId">The expected default configuration identifier.</param>
    /// <param name="response">The simulated IPC response.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [MemberData(nameof(GetTestCases))]
    public async Task It_should_ExecuteCommandCorrectly(string cmd, string defaultConfigId, Response response)
    {
        // Arrange
        var expectedCmd = IpcCommand.CreateNew(defaultConfigId);
        var parseResult = _rootCommand.Parse(cmd);
        _commandSenderMock.Setup(m => m.SendAsync(It.IsAny<IpcCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await parseResult.InvokeAsync();

        // Assert
        _commandSenderMock.Verify(
            m => m.SendAsync(
                It.Is<IpcCommand>(c => c.DefaultSessionConfigId == expectedCmd.DefaultSessionConfigId),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _messageWriterMock.Verify(
            m => m.WriteLineAsync(!response.Success, response.Message, It.IsAny<CancellationToken>()));
    }
}