using System.CommandLine;

using Moq;

using Tiempito.CLI.Commands.Config;
using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Models;
using Tiempito.IPC.Models.Enums;

using IpcCommand = Tiempito.IPC.Models.Commands.Config.UserFeatureConfigCommand;

namespace Tiempito.CLI.UnitTests.Commands.Config;

/// <summary>
/// Unit tests for the <see cref="UserFeatureConfigCommand"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature ", "CLI")]
public sealed class UserFeatureConfigCommandTests : IDisposable
{
    private readonly MockRepository _mockRepository;
    private readonly Mock<ICommandSender> _commandSenderMock;
    private readonly Mock<IMessageWriter> _messageWriterMock;
    private readonly RootCommand _rootCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserFeatureConfigCommandTests"/> class.
    /// </summary>
    public UserFeatureConfigCommandTests()
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
    public static TheoryData<string, bool, UserFeature, Response> GetTestCases()
    {
        return new TheoryData<string, bool, UserFeature, Response>
        {
            { "config enable notification", true, UserFeature.Notification, Response.Ok(Guid.Empty, "Test message") },
            { "config enable nc", true, UserFeature.Notification, Response.Ok(Guid.Empty, "Test message") },
            { "config disable notification", false, UserFeature.Notification, Response.Ok(Guid.Empty, "Test message") },
            { "config disable nc", false, UserFeature.Notification, Response.Ok(Guid.Empty, "Test message") },
            {
                "config enable notification",
                true,
                UserFeature.Notification,
                Response.BadRequest(Guid.Empty, "Test message")
            },
            {
                "config disable notification",
                false,
                UserFeature.Notification,
                Response.BadRequest(Guid.Empty, "Test message")
            },
        };
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Verifies that the command executes correctly and calls the command sender with the expected parameters.
    /// </summary>
    /// <param name="cmd">The command string to parse.</param>
    /// <param name="enable">The expected enable state.</param>
    /// <param name="feature">The expected feature to configure.</param>
    /// <param name="response">The simulated response from the command sender.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [MemberData(nameof(GetTestCases))]
    public async Task It_should_ExecuteCommandCorrectly(string cmd, bool enable, UserFeature feature, Response response)
    {
        // Arrange
        var expectedCmd = IpcCommand.CreateNew(enable, feature);
        var parseResult = _rootCommand.Parse(cmd);
        _commandSenderMock.Setup(m => m.SendAsync(It.IsAny<IpcCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await parseResult.InvokeAsync();

        // Assert
        _commandSenderMock.Verify(
            m => m.SendAsync(
                It.Is<IpcCommand>(c => c.Enable == expectedCmd.Enable && c.Feature == expectedCmd.Feature),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _messageWriterMock.Verify(
            m => m.WriteLineAsync(!response.Success, response.Message, It.IsAny<CancellationToken>()));
    }
}