using Tiempito.IPC.Models.Commands.Config;

namespace Tiempito.IPC.UnitTests.Models.Commands.Config;

/// <summary>
/// Unit tests for the <see cref="SetConfigCommand"/> record.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "IPC")]
public sealed record SetConfigCommandTests
{
    /// <summary>
    /// Tests that <see cref="SetConfigCommand.CreateNew"/> returns a command with the specified DefaultSessionConfigId.
    /// </summary>
    /// <param name="defaultSessionConfigId">The session config ID to set as default.</param>
    [Theory]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("3")]
    public void CreateNew_should_ReturnCommandWithSpecifiedDefaultSessionConfigId(string defaultSessionConfigId)
    {
        // Act
        var actual = SetConfigCommand.CreateNew(defaultSessionConfigId);

        // Assert
        Assert.Equal(defaultSessionConfigId, actual.DefaultSessionConfigId);
    }

    /// <summary>
    /// Tests that <see cref="SetConfigCommand.CreateNew"/> creates commands with
    /// unique random UUIDs for ID and CorrelationId.
    /// </summary>
    [Fact]
    public void CreateNew_should_CreateCommandWithRandomUuid()
    {
        // Act
        var cmd1 = SetConfigCommand.CreateNew("1");
        var cmd2 = SetConfigCommand.CreateNew("1");

        // Assert
        Assert.NotEqual(cmd1.Id, cmd2.Id);
        Assert.NotEqual(cmd1.CorrelationId, cmd2.CorrelationId);
    }

    /// <summary>
    /// Tests that <see cref="SetConfigCommand.CreateNew"/> throws an <see cref="ArgumentNullException"/>
    /// when the session config ID is <c>null</c>.
    /// </summary>
    [Fact]
    public void CreateNew_should_ThrowArgumentNullException_when_SessionConfigIdIsNull()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => SetConfigCommand.CreateNew(defaultSessionConfigId: null!));
    }
}