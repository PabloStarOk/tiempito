using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.IPC.UnitTests.Models.Commands.Session;

/// <summary>
/// Unit tests for the <see cref="StartSessionCommand"/> record.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "IPC")]
public sealed class StartSessionCommandTests
{
    /// <summary>
    /// Tests that <see cref="StartSessionCommand.CreateNew"/> returns a command with the given session and config IDs.
    /// </summary>
    /// <param name="id">The session ID to use.</param>
    /// <param name="configId">The session config ID to use.</param>
    [Theory]
    [InlineData("1", "config1")]
    [InlineData("2", "config2")]
    [InlineData("3", "config3")]
    public void CreateNew_should_ReturnCommandWithGivenIdAndConfigId(string id, string configId)
    {
        // Act
        var actual = StartSessionCommand.CreateNew(id, configId);

        // Assert
        Assert.Equal(id, actual.SessionId);
        Assert.Equal(configId, actual.SessionConfigId);
    }

    /// <summary>
    /// Tests that <see cref="StartSessionCommand.CreateNew"/> returns a command with an empty session ID when the ID is not specified.
    /// </summary>
    [Fact]
    public void CreateNew_should_ReturnCommandWithEmptyId_when_IdIsNotSpecified()
    {
        // Act
        var actual = StartSessionCommand.CreateNew(sessionConfigId: "1");

        // Assert
        Assert.Empty(actual.SessionId);
    }

    /// <summary>
    /// Tests that <see cref="StartSessionCommand.CreateNew"/> returns a command with an empty config ID when the config ID is not specified.
    /// </summary>
    [Fact]
    public void CreateNew_should_ReturnCommandWithEmptyConfigId_when_ConfigIdIsNotSpecified()
    {
        // Act
        var actual = StartSessionCommand.CreateNew(sessionId: "1");

        // Assert
        Assert.Empty(actual.SessionConfigId);
    }

    /// <summary>
    /// Tests that <see cref="StartSessionCommand.CreateNew"/> creates commands with random UUIDs for ID and CorrelationId.
    /// </summary>
    [Fact]
    public void CreateNew_should_CreateCommandWithRandomUuid()
    {
        // Act
        var cmd1 = StartSessionCommand.CreateNew();
        var cmd2 = StartSessionCommand.CreateNew();

        // Assert
        Assert.NotEqual(cmd1.Id, cmd2.Id);
        Assert.NotEqual(cmd1.CorrelationId, cmd2.CorrelationId);
    }

    /// <summary>
    /// Tests that <see cref="StartSessionCommand.CreateNew"/> throws an <see cref="ArgumentNullException"/> when the session ID is null.
    /// </summary>
    [Fact]
    public void CreateNew_should_ReturnArgumentNullException_when_GivenIdIsNull()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => StartSessionCommand.CreateNew(sessionId: null!));
    }

    /// <summary>
    /// Tests that <see cref="StartSessionCommand.CreateNew"/> throws an <see cref="ArgumentNullException"/> when the session config ID is null.
    /// </summary>
    [Fact]
    public void CreateNew_should_ReturnArgumentNullException_when_GivenConfigIdIsNull()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => StartSessionCommand.CreateNew(sessionConfigId: null!));
    }
}