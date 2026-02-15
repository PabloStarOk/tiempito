using System.IO.Abstractions;
using System.Threading.Channels;

using IniParser;

using Microsoft.Extensions.FileProviders;

using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Config.Sessions;
using Tiempito.Daemon.Application.Config.User;
using Tiempito.Daemon.Application.Notifications;
using Tiempito.Daemon.Application.Sessions;
using Tiempito.Daemon.Infrastructure.Config;
using Tiempito.Daemon.Infrastructure.Config.Sessions;
using Tiempito.Daemon.Infrastructure.Config.User;
using Tiempito.Daemon.Infrastructure.Notifications;
using Tiempito.Daemon.Infrastructure.Sessions;
using Tiempito.IPC.Models;
#if LINUX
using Tiempito.Daemon.Infrastructure.Notifications.Linux;
#elif WINDOWS10_0_17763_0_OR_GREATER
using Tiempito.Daemon.Infrastructure.Notifications.Windows;
#endif

namespace Tiempito.Daemon.Infrastructure;

/// <summary>
/// Provides extension methods for registering infrastructure services in the dependency injection container.
/// </summary>
internal static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure services required by the application.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration manager for accessing configuration files.</param>
    public static void AddInfrastructure(this IServiceCollection services, IConfigurationManager configuration)
    {
        services.AddSingleton(TimeProvider.System);
        AddConfigServices(services, configuration);
        AddSessionServices(services);
        AddNotificationServices(services);
    }

    private static void AddConfigServices(IServiceCollection services, IConfigurationManager configuration)
    {
        services.AddSingleton<IFileSystem>(_ => new FileSystem());
        services.AddSingleton<IAppFilesystemPathProvider, AppFilesystemPathProvider>();
        services.AddSingleton<IFileProvider>(sp =>
        {
            var appFilesystem = sp.GetRequiredService<IAppFilesystemPathProvider>();
            return new PhysicalFileProvider(appFilesystem.UserConfigDirectoryPath);
        });

        var userConfigFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            AppConfigConstants.RootConfigDirName,
            AppConfigConstants.UserConfigFileName);
        configuration.AddIniFile(userConfigFilePath, optional: false, reloadOnChange: true);
        services.AddSingleton(_ => new StreamIniDataParser());

        using (ServiceProvider sp = services.BuildServiceProvider())
        {
            var appFilesystem = sp.GetRequiredService<IAppFilesystemPathProvider>();
            configuration.AddIniFile(appFilesystem.DaemonConfigFilePath, optional: false, reloadOnChange: true);
        }

        services.AddSingleton<ITimeSpanConverter, TimeSpanConverter>();
        services.AddSingleton<IUserConfigReader, UserConfigReader>();
        services.AddSingleton<IUserConfigWriter, UserConfigWriter>();
        services.AddSingleton<ISessionConfigReader, SessionConfigReader>();
        services.AddSingleton<ISessionConfigWriter, SessionConfigWriter>();
    }

    private static void AddSessionServices(IServiceCollection services)
    {
        services.AddSingleton<ISessionFactory, SessionFactory>();
    }

    private static void AddNotificationServices(IServiceCollection services)
    {
#if LINUX
        if (OperatingSystem.IsLinux())
        {
            services.AddSystemd();
            services.AddLinuxNotificationsDbus();
            services.AddSingleton<ILinuxSoundPlayer, LinuxSoundPlayer>();
            services.AddSingleton<ILinuxNotifier, LinuxNotifier>();
            services.AddTransient<ILinuxNotificationIconLoader, LinuxNotificationIconLoader>();
            services.AddSingleton<LinuxNotificationService>();
            services.AddSingleton<INotificationService>(sp => sp.GetRequiredService<LinuxNotificationService>());
            services.AddHostedService(sp => sp.GetRequiredService<LinuxNotificationService>());
            services.AddLogging(builder =>
            {
                builder.AddSystemdConsole();
            });
        }
#elif WINDOWS10_0_17763_0_OR_GREATER
        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240))
        {
            services.AddWindowsService();
            services.AddSingleton<INotificationService, WindowsNotificationService>();
        }
#endif

        services.AddSingleton(_ =>
        {
            var channelOptions = new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
            };
            var channel = Channel.CreateUnbounded<Message>(channelOptions);
            return new StandardOutQueue(channel);
        });
        services.AddSingleton<IStandardOutQueueWriter>(sp => sp.GetRequiredService<StandardOutQueue>());
        services.AddSingleton<IStandardOutQueueReader>(sp => sp.GetRequiredService<StandardOutQueue>());
    }
}