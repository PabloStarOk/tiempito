using Tiempito.IPC.Models.Commands.Config;
using Tiempito.IPC.Models.Enums;

namespace Tiempito.IPC.UnitTests.Models.Commands.Config;

/// <summary>
/// Unit tests for the <see cref="UserFeatureConfigCommand"/> record.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "IPC")]
public sealed class UserFeatureConfigCommandTests
{
    /// <summary>
    /// Tests that <see cref="UserFeatureConfigCommand.CreateNew"/> returns a command with the specified parameters.
    /// </summary>
    /// <param name="enable">Whether the feature should be enabled.</param>
    /// <param name="feature">The user feature to configure.</param>
    [Theory]
    [InlineData(true, UserFeature.Notification)]
    [InlineData(false, UserFeature.Notification)]
    public void CreateNew_should_ReturnCommandWithSpecifiedParameters(bool enable, UserFeature feature)
    {
        // Act
        var actual = UserFeatureConfigCommand.CreateNew(enable, feature);

        // Assert
        Assert.Equal(enable, actual.Enable);
        Assert.Equal(feature, actual.Feature);
    }

    /// <summary>
    /// Tests that <see cref="UserFeatureConfigCommand.CreateNew"/> generates commands with unique random UUIDs for Id and CorrelationId.
    /// </summary>
    [Fact]
    public void CreateNew_should_CreateCommandWithRandomUuid()
    {
        // Act
        const bool enable = true;
        const UserFeature feature = UserFeature.Notification;
        var cmd1 = UserFeatureConfigCommand.CreateNew(enable, feature);
        var cmd2 = UserFeatureConfigCommand.CreateNew(enable, feature);

        // Assert
        Assert.NotEqual(cmd1.Id, cmd2.Id);
        Assert.NotEqual(cmd1.CorrelationId, cmd2.CorrelationId);
    }
}