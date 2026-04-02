using Moq;

using Tiempito.CLI.Commands.Session;
using Tiempito.CLI.Services.Abstractions;
using Tiempito.IPC.Models.Commands.Session;

using StartSessionCommand = Tiempito.CLI.Commands.Session.StartSessionCommand;

namespace Tiempito.CLI.UnitTests.Commands.Session;

/// <summary>
/// Unit tests for the <see cref="SessionCommand"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature ", "CLI")]
public class SessionCommandTests
{
    private readonly Mock<ICommandSender> _commandSenderMock;
    private readonly Mock<IMessageWriter> _messageWriterMock;
    private readonly Mock<ISessionFollower> _sessionFollowerMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionCommandTests"/> class.
    /// </summary>
    public SessionCommandTests()
    {
        _commandSenderMock = new Mock<ICommandSender>();
        _messageWriterMock = new Mock<IMessageWriter>();
        _sessionFollowerMock = new Mock<ISessionFollower>();
    }

    /// <summary>
    /// Tests that the <see cref="SessionCommand"/> contains the expected subcommands.
    /// </summary>
    [Fact]
    public void It_should_ContainExpectedSubCommands()
    {
        // Arrange
        var cmd = new SessionCommand(_commandSenderMock.Object, _messageWriterMock.Object, _sessionFollowerMock.Object);

        // Assert
        Assert.Contains(cmd.Subcommands, c => c is StartSessionCommand);
        Assert.Contains(cmd.Subcommands, c => c is GenericSessionCommand<CancelSessionCommand>);
        Assert.Contains(cmd.Subcommands, c => c is GenericSessionCommand<PauseSessionCommand>);
        Assert.Contains(cmd.Subcommands, c => c is GenericSessionCommand<ResumeSessionCommand>);
    }
}