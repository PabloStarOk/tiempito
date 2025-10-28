using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Salaros.Configuration;
using System.IO.Pipes;
using Tiempito.Daemon;
using Tiempito.Daemon.Application;
using Tiempito.Daemon.Application.Config;
using Tiempito.Daemon.Application.Notifications;
using Tiempito.Daemon.Application.Sessions;
using Tiempito.Daemon.Application.Shared.Abstractions;
using Tiempito.Daemon.Domain.Sessions;
using Tiempito.Daemon.Infrastructure.Config;
using Tiempito.Daemon.Infrastructure.Sessions;
using Tiempito.Daemon.Server;
using Tiempito.Daemon.Server.Configuration;
#if LINUX
using Tiempito.Daemon.Infrastructure.Notifications.Linux;
#elif WINDOWS10_0_17763_0_OR_GREATER
using Tiempito.Daemon.Infrastructure.Notifications.Windows;
#endif
using Tiempito.IPC;

using TimeSpanConverter = Tiempito.Daemon.Infrastructure.Config.TimeSpanConverter;

var builder = Host.CreateApplicationBuilder(args);

IServiceProvider serviceProvider = builder.Services.BuildServiceProvider();
var loggerProvider = serviceProvider.GetRequiredService<ILoggerFactory>();

// Add filesystem providers.
IAppFilesystemPathProvider appFilesystemPathProvider = new AppFilesystemPathProvider(loggerProvider.CreateLogger<AppFilesystemPathProvider>());
builder.Services.AddSingleton(appFilesystemPathProvider);

IFileProvider userConfigFileProvider = new PhysicalFileProvider(appFilesystemPathProvider.UserConfigDirectoryPath);
builder.Services.AddKeyedSingleton(
    AppConfigConstants.UserConfigFileProviderKey,
    userConfigFileProvider);
builder.Services.AddKeyedSingleton(
    AppConfigConstants.UserConfigParserServiceKey,
    new ConfigParser(userConfigFileProvider.GetFileInfo(AppConfigConstants.UserConfigFileName).PhysicalPath)); // BUG: If two section names are equals throws an exception.

// Add configuration services.
builder.Services.AddConfigurationServices();

// Load daemon config
builder.Configuration.AddIniFile(appFilesystemPathProvider.DaemonConfigFilePath, optional: false, reloadOnChange: true);
builder.Services.Configure<PipeConfig>(builder.Configuration.GetRequiredSection(key: PipeConfig.Pipe));
builder.Services.Configure<NotificationConfig>(builder.Configuration.GetSection(key: NotificationConfig.Notification));

// Add IPC dependencies
builder.Services.AddIpc();

// Add system notification
#if LINUX
if (OperatingSystem.IsLinux())
{
    builder.Services.AddLinuxNotificationsDbus();
    builder.Services.AddTransient<ISystemAsyncIconLoader, LinuxSystemIconLoader>();
    builder.Services.AddSingleton<ISystemSoundPlayer, LinuxSystemSoundPlayer>();
    builder.Services.AddTransient<ISystemNotifier, LinuxNotifier>();
}
#elif WINDOWS10_0_17763_0_OR_GREATER
if (OperatingSystem.IsWindowsVersionAtLeast(10,0,10240))
    builder.Services.AddTransient<ISystemNotifier, WindowsNotifier>();
#endif

// Add server named pipe.
builder.Services.AddSingleton(sp =>
    {
        var options = sp.GetRequiredService<IOptions<PipeConfig>>();
        return new NamedPipeServerStream
        (
            options.Value.PipeName,
            options.Value.PipeDirection,
            options.Value.PipeMaxInstances,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous
        );
    } );

// Add stdout handler

builder.Services.AddSingleton<TextWriter>(sp =>
    {
        var stream = sp.GetRequiredService<NamedPipeServerStream>();
        return new StreamWriter(stream);
    });
builder.Services.AddSingleton(sp =>
    {
        var pipeStdOut = sp.GetRequiredService<TextWriter>();
        return new StandardOutMessageProcessor([], pipeStdOut);
    }
);
builder.Services.AddSingleton<IStandardOutSink>(sp => sp.GetRequiredService<StandardOutMessageProcessor>());
builder.Services.AddSingleton<IStandardOutQueue>(sp => sp.GetRequiredService<StandardOutMessageProcessor>());

// Add time provider.
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddApplication();

// Server dependencies
builder.Services.AddSingleton<ITimeSpanConverter, TimeSpanConverter>();
builder.Services.AddSingleton<IServer, Server>();

// Session service dependencies
var sessionProgress = new Progress<Session>();
builder.Services.AddSingleton(sessionProgress);
builder.Services.AddSingleton<IProgress<Session>>(sessionProgress);
builder.Services.AddSingleton<ISessionStorage, SessionStorage>();
builder.Services.AddKeyedSingleton(typeof(TimeSpan), "TimingInterval", (_, _) => TimeSpan.FromSeconds(1));
builder.Services.AddSingleton<ISessionTimer, SessionTimer>();

builder.Services.AddSingleton<SessionService>();
builder.Services.AddSingleton<NotificationService>();

builder.Services.AddSingleton<ISessionService>(sp => sp.GetService<SessionService>()!);
builder.Services.AddSingleton<INotificationService>(sp => sp.GetService<NotificationService>()!);

builder.Services.AddSingleton<Service>(sp => sp.GetService<SessionService>()!);
builder.Services.AddSingleton<Service>(sp => sp.GetService<NotificationService>()!);

builder.Services.AddHostedService<DaemonWorker>();

var host = builder.Build();
host.Run();
