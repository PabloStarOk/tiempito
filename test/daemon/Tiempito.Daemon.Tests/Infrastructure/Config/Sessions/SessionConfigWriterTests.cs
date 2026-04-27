using System.IO.Abstractions.TestingHelpers;

using IniParser;
using IniParser.Model;
using Moq;

using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Shared;
using Tiempito.Daemon.Domain.Config;
using Tiempito.Daemon.Infrastructure.Config;
using Tiempito.Daemon.Infrastructure.Config.Sessions;
using Tiempito.Daemon.Tests.Sessions.Helpers;

namespace Tiempito.Daemon.Tests.Infrastructure.Config.Sessions;

/// <summary>
/// Unit tests for the <see cref="SessionConfigWriter"/> class.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "Config")]
public sealed class SessionConfigWriterTests : IDisposable
{
    private readonly MockRepository _mockRepository;
    private readonly MockFileSystem _fileSystemMock;
    private readonly SessionConfigWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionConfigWriterTests"/> class.
    /// </summary>
    public SessionConfigWriterTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Loose);
        _fileSystemMock = new MockFileSystem();
        var iniParser = new StreamIniDataParser();
        _writer = new SessionConfigWriter(_fileSystemMock, iniParser, new TimeSpanConverter());
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _mockRepository.VerifyAll();
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigWriter.Write"/> writes a new session config to the file when the file exists.
    /// </summary>
    [Fact]
    public void Write_should_WriteNewSessionConfigToFile_when_FileExists()
    {
        // Arrange
        var newConfig = new SessionConfig(
            TargetCycles: 2,
            Id: "Test",
            DelayBetweenTimes: TimeSpan.FromMinutes(1),
            FocusDuration: TimeSpan.FromMinutes(2),
            BreakDuration: TimeSpan.FromMinutes(3));
        var mockFileData = new MockFileData(string.Empty);
        var sectionName = $"{AppConfigConstants.SessionSectionPrefix}:{newConfig.Id}";
        var iniData = new IniData();
        var expectedSectionData = new SectionData(sectionName);
        expectedSectionData.Keys.AddKey(nameof(SessionConfig.TargetCycles), newConfig.TargetCycles.ToString());
        expectedSectionData.Keys.AddKey(nameof(SessionConfig.DelayBetweenTimes), "1m");
        expectedSectionData.Keys.AddKey(nameof(SessionConfig.FocusDuration), "2m");
        expectedSectionData.Keys.AddKey(nameof(SessionConfig.BreakDuration), "3m");
        iniData.Sections.SetSectionData(sectionName, expectedSectionData);
        _fileSystemMock.AddFile(Paths.UserConfigFilePath, mockFileData);

        // Act
        bool actual = _writer.Write(AppConfigConstants.SessionSectionPrefix, newConfig);

        // Assert
        Assert.True(actual);
        Assert.Equal(iniData.ToString(), mockFileData.TextContents);
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigWriter.Write"/> throws an <see cref="ArgumentException"/>
    /// when the given section prefix is invalid (null, empty, or whitespace).
    /// </summary>
    /// <param name="sectionPrefix">The session section prefix to test for validity.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Write_should_ThrowArgumentException_when_GivenSectionPrefixIsInvalid(string? sectionPrefix)
    {
        // Arrange
        var stubConfig = SessionProvider.CreateConfig();

        // Assert
        Assert.ThrowsAny<ArgumentException>(() => _writer.Write(sectionPrefix!, stubConfig));
    }

    /// <summary>
    /// Tests that <see cref="SessionConfigWriter.Write"/> returns false when the config file does not exist.
    /// </summary>
    [Fact]
    public void Write_should_ReturnFalse_when_FileDoesNotExist()
    {
        // Arrange
        var stubConfig = SessionProvider.CreateConfig();

        // Act
        bool actual = _writer.Write(AppConfigConstants.SessionSectionPrefix, stubConfig);

        // Assert
        Assert.False(actual);
    }
}