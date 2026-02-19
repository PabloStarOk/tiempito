using System.Collections.Immutable;
using System.IO.Abstractions.TestingHelpers;

using IniParser;
using IniParser.Model;

using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Shared;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Infrastructure.Config.User;
using Tiempito.IPC.Models.Enums;

namespace Tiempito.Daemon.Tests.Infrastructure.Config.User;

/// <summary>
/// Unit tests for <see cref="UserConfigWriter"/>.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature ", "Config")]
public sealed class UserConfigWriterTests
{
    private readonly MockFileSystem _mockFileSystem;
    private readonly UserConfigWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConfigWriterTests"/> class.
    /// </summary>
    public UserConfigWriterTests()
    {
        _mockFileSystem = new MockFileSystem();
        var iniParser = new StreamIniDataParser();
        _writer = new UserConfigWriter(_mockFileSystem, iniParser);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigWriter.Write"/> writes the configuration to the file
    /// when the file exists, using various combinations of <paramref name="defaultConfigId"/>
    /// and <paramref name="userFeature"/>.
    /// </summary>
    /// <param name="defaultConfigId">The default configuration ID to write, or <c>null</c>.</param>
    /// <param name="userFeature">The user feature to enable, or <c>null</c>.</param>
    [Theory]
    [InlineData("1", UserFeature.Notification)]
    [InlineData("2", null)]
    [InlineData(null, UserFeature.Notification)]
    [InlineData(null, null)]
    public void Write_should_WriteConfigurationToFile_when_FileExists(string? defaultConfigId, UserFeature? userFeature)
    {
        // Arrange
        UserFeature[] enabledFeatures = userFeature is null ? [] : [(UserFeature)userFeature];
        var userConfig = new UserConfig(defaultConfigId, enabledFeatures.ToImmutableHashSet());
        var iniData = new IniData();
        var sectionData = new SectionData(AppConfigConstants.UserSectionName);
        var mockFileData = new MockFileData(contents: []);
        sectionData.Keys.AddKey(nameof(UserConfig.DefaultConfigId), userConfig.DefaultConfigId);
        sectionData.Keys.AddKey(nameof(UserConfig.EnabledFeatures), string.Join(',', userConfig.EnabledFeatures));
        iniData.Sections.Add(sectionData);
        _mockFileSystem.AddFile(Paths.UserConfigFilePath, mockFileData);

        // Act
        bool actual = _writer.Write(userConfig);

        // Assert
        Assert.True(actual);
        Assert.Equal(iniData.ToString(), mockFileData.TextContents);
    }

    /// <summary>
    /// Tests that <see cref="UserConfigWriter.Write"/> returns <c>false</c> when the user
    /// configuration file does not exist on disk.
    /// </summary>
    [Fact]
    public void Write_should_ReturnFalse_when_FileDoesNotExist()
    {
        // Arrange
        var userConfig = new UserConfig(DefaultConfigId: string.Empty, []);

        // Act
        bool actual = _writer.Write(userConfig);

        // Assert
        Assert.False(actual);
    }
}