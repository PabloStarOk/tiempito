using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.IPC.UnitTests.Models.Commands;

/// <summary>
/// Unit tests for the <see cref="CancelSessionCommand"/> record.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "IPC")]
public sealed class CancelSessionCommandTests
{
    /// <summary>
    /// Tests that <see cref="CancelSessionCommand.CreateNew"/> returns a command with the given session ID.
    /// </summary>
    /// <param name="sessionId">The session ID to use.</param>
    [Theory]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("3")]
    public void CreateNew_should_ReturnCommandWithGivenIdAndConfigId(string sessionId)
    {
        // Act
        var actual = CancelSessionCommand.CreateNew(sessionId);

        // Assert
        Assert.Equal(sessionId, actual.SessionId);
    }

    /// <summary>
    /// Tests that <see cref="CancelSessionCommand.CreateNew"/> returns a command with an empty session ID when no session ID is specified.
    /// </summary>
    [Fact]
    public void CreateNew_should_ReturnCommandWithEmptySessionId_when_SessionIdIsNotSpecified()
    {
        // Act
        var actual = CancelSessionCommand.CreateNew();

        // Assert
        Assert.Empty(actual.SessionId);
    }

    /// <summary>
    /// Tests that <see cref="CancelSessionCommand.CreateNew"/> creates commands with random UUIDs for ID and CorrelationId.
    /// </summary>
    [Fact]
    public void CreateNew_should_CreateCommandWithRandomUuid()
    {
        // Act
        var cmd1 = CancelSessionCommand.CreateNew();
        var cmd2 = CancelSessionCommand.CreateNew();

        // Assert
        Assert.NotEqual(cmd1.Id, cmd2.Id);
        Assert.NotEqual(cmd1.CorrelationId, cmd2.CorrelationId);
    }

    /// <summary>
    /// Tests that <see cref="CancelSessionCommand.CreateNew"/> throws an <see cref="ArgumentNullException"/> when the session ID is null.
    /// </summary>
    [Fact]
    public void CreateNew_should_ReturnArgumentNullException_when_GivenIdIsNull()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => CancelSessionCommand.CreateNew(sessionId: null!));
    }
}