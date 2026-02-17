using System.IO.Abstractions;

using IniParser;
using IniParser.Model;

using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Config.User;
using Tiempito.Daemon.Application.Shared;
using Tiempito.Daemon.Domain.Config;

namespace Tiempito.Daemon.Infrastructure.Config.User;

/// <summary>
/// Writes in the user's configuration.
/// </summary>
public class UserConfigWriter : IUserConfigWriter
{
    private readonly IFileSystem _fileSystem;
    private readonly StreamIniDataParser _iniParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConfigWriter"/> class.
    /// </summary>
    /// <param name="fileSystem">The file system abstraction to use.</param>
    /// <param name="iniParser">The INI data parser to use.</param>
    public UserConfigWriter(IFileSystem fileSystem, StreamIniDataParser iniParser)
    {
        _fileSystem = fileSystem;
        _iniParser = iniParser;
    }

    /// <inheritdoc/>
    public bool Write(UserConfig userConfig)
    {
        if (!_fileSystem.File.Exists(Paths.UserConfigFilePath))
        {
            return false;
        }

        using var configFile =
            _fileSystem.File.Open(Paths.UserConfigFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
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

        string enabledFeatures = string.Join(AppConfigConstants.IniArraySeparator, userConfig.EnabledFeatures);
        var sectionData = new SectionData(AppConfigConstants.UserSectionName);
        sectionData.Keys.AddKey(nameof(UserConfig.DefaultConfigId), userConfig.DefaultConfigId);
        sectionData.Keys.AddKey(nameof(UserConfig.EnabledFeatures), enabledFeatures);
        iniData.Sections.SetSectionData(sectionData.SectionName, sectionData);

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
