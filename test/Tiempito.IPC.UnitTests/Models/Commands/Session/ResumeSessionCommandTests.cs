using Tiempito.IPC.Models.Commands.Session;

namespace Tiempito.IPC.UnitTests.Models.Commands.Session;

/// <summary>
/// Unit tests for the <see cref="ResumeSessionCommand"/> record.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "IPC")]
public sealed class ResumeSessionCommandTests
{
    /// <summary>
    /// Tests that <see cref="ResumeSessionCommand.CreateNew"/> returns a command with the given session ID.
    /// </summary>
    /// <param name="sessionId">The session ID to use.</param>
    [Theory]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("3")]
    public void CreateNew_should_ReturnCommandWithGivenIdAndConfigId(string sessionId)
    {
        // Act
        var actual = ResumeSessionCommand.CreateNew(sessionId);

        // Assert
        Assert.Equal(sessionId, actual.SessionId);
    }

    /// <summary>
    /// Tests that <see cref="ResumeSessionCommand.CreateNew"/> returns a command with an empty session ID when no session ID is specified.
    /// </summary>
    [Fact]
    public void CreateNew_should_ReturnCommandWithEmptySessionId_when_SessionIdIsNotSpecified()
    {
        // Act
        var actual = ResumeSessionCommand.CreateNew();

        // Assert
        Assert.Empty(actual.SessionId);
    }

    /// <summary>
    /// Tests that <see cref="ResumeSessionCommand.CreateNew"/> creates commands with random UUIDs for ID and CorrelationId.
    /// </summary>
    [Fact]
    public void CreateNew_should_CreateCommandWithRandomUuid()
    {
        // Act
        var cmd1 = ResumeSessionCommand.CreateNew();
        var cmd2 = ResumeSessionCommand.CreateNew();

        // Assert
        Assert.NotEqual(cmd1.Id, cmd2.Id);
        Assert.NotEqual(cmd1.CorrelationId, cmd2.CorrelationId);
    }

    /// <summary>
    /// Tests that <see cref="ResumeSessionCommand.CreateNew"/> throws an <see cref="ArgumentNullException"/> when the session ID is null.
    /// </summary>
    [Fact]
    public void CreateNew_should_ReturnArgumentNullException_when_GivenIdIsNull()
    {
        // Assert
        Assert.Throws<ArgumentNullException>(() => ResumeSessionCommand.CreateNew(sessionId: null!));
    }
}