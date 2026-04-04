using Moq;

using Tiempito.CLI.Commands.Config;
using Tiempito.CLI.Services.Abstractions;

namespace Tiempito.CLI.UnitTests.Commands.Config;

/// <summary>
/// Unit tests for the <see cref="ConfigCommand"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature ", "CLI")]
public sealed class ConfigCommandTests
{
    private readonly Mock<ICommandSender> _commandSenderMock;
    private readonly Mock<IMessageWriter> _messageWriterMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigCommandTests"/> class.
    /// </summary>
    public ConfigCommandTests()
    {
        _commandSenderMock = new Mock<ICommandSender>();
        _messageWriterMock = new Mock<IMessageWriter>();
    }

    /// <summary>
    /// Tests that the <see cref="ConfigCommand"/> contains the expected subcommands.
    /// </summary>
    [Fact]
    public void It_should_ContainExpectedSubCommands()
    {
        // Arrange
        var cmd = new ConfigCommand(_commandSenderMock.Object, _messageWriterMock.Object);

        // Assert
        Assert.Contains(cmd.Subcommands, c => c is CreateSessionConfigCommand);
        Assert.Contains(cmd.Subcommands, c => c is ModifySessionConfigCommand);
        Assert.Contains(cmd.Subcommands, c => c is SetConfigCommand);
        Assert.Contains(cmd.Subcommands, c => c is UserFeatureConfigCommand);
    }
}