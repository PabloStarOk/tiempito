using Tiempito.Daemon.Application.Config;

namespace Tiempito.Daemon.Application.Shared;

internal static class Paths
{
    private static readonly string CommonDataDir =
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

    private static readonly string UserDataDir =
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    public static readonly string DaemonConfigDirectoryPath = Path.Combine(
        CommonDataDir,
        AppConfigConstants.RootConfigDirName);

    public static readonly string DaemonConfigFilePath = Path.Combine(
        DaemonConfigDirectoryPath,
        AppConfigConstants.DaemonConfigFileName);

    public static readonly string UserConfigFilePath = Path.Combine(
        UserDataDir,
        AppConfigConstants.RootConfigDirName,
        AppConfigConstants.UserConfigFileName);

    public static readonly string ApplicationIconPath = Path.Combine(
        DaemonConfigDirectoryPath,
        AppConfigConstants.IconFileName);
}