using System.IO.Abstractions;

using IniParser;
using IniParser.Model;

using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Config.Sessions;
using Tiempito.Daemon.Domain.Config;

namespace Tiempito.Daemon.Infrastructure.Config.Sessions;

/// <summary>
/// Provides write operations to save <see cref="SessionConfig"/> in user's config file.
/// </summary>
public class SessionConfigWriter : ISessionConfigWriter
{
    private readonly IFileSystem _fileSystem;
    private readonly StreamIniDataParser _iniParser;
    private readonly ITimeSpanConverter _timeSpanConverter;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionConfigWriter"/> class.
    /// </summary>
    /// <param name="fileSystem">An <see cref="IFileSystem"/> for file operations.</param>
    /// <param name="iniParser">A <see cref="StreamIniDataParser"/> for parsing INI data streams.</param>
    /// <param name="timeSpanConverter">A <see cref="ITimeSpanConverter"/> to convert <see cref="TimeSpan"/> to string values.</param>
    public SessionConfigWriter(
        IFileSystem fileSystem,
        StreamIniDataParser iniParser,
        ITimeSpanConverter timeSpanConverter)
    {
        _fileSystem = fileSystem;
        _iniParser = iniParser;
        _timeSpanConverter = timeSpanConverter;
    }

    /// <inheritdoc/>
    public bool Write(string prefixSectionName, SessionConfig sessionConfig)
    {
        var configFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            AppConfigConstants.RootConfigDirName,
            AppConfigConstants.UserConfigFileName);

        if (!_fileSystem.File.Exists(configFilePath))
        {
            return false;
        }

        using var configFile = _fileSystem.File.Open(configFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        IniData iniData;
        try
        {
            using var streamReader = new StreamReader(configFile, leaveOpen: true);
            iniData = _iniParser.ReadData(streamReader);
        }
        catch (IOException)
        {
            return false;
        }

        var targetCycles = sessionConfig.TargetCycles.ToString();
        var delayBetweenTimes = _timeSpanConverter.Format(sessionConfig.DelayBetweenTimes);
        string focusDuration = _timeSpanConverter.Format(sessionConfig.FocusDuration);
        string breakDuration = _timeSpanConverter.Format(sessionConfig.BreakDuration);

        string sectionName = $"{prefixSectionName}{AppConfigConstants.NestedSectionSeparator}{sessionConfig.Id}";
        var section = new SectionData(sectionName);
        section.Keys.AddKey(nameof(SessionConfig.TargetCycles), targetCycles);
        section.Keys.AddKey(nameof(SessionConfig.DelayBetweenTimes), delayBetweenTimes);
        section.Keys.AddKey(nameof(SessionConfig.FocusDuration), focusDuration);
        section.Keys.AddKey(nameof(SessionConfig.BreakDuration), breakDuration);
        iniData.Sections.SetSectionData(section.SectionName, section);

        try
        {
            configFile.Position = 0;
            configFile.SetLength(0);
            using var streamWriter = new StreamWriter(configFile);
            _iniParser.WriteData(streamWriter, iniData);
        }
        catch (IOException)
        {
            return false;
        }

        return true;
    }
}
