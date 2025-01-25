using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Salaros.Configuration;
using System.IO.Pipes;
using System.Text.Json;
using Tiempito.IPC.NET.Packets;
using Tiempitod.NET;
using Tiempitod.NET.Commands;
using Tiempitod.NET.Configuration;
using Tiempitod.NET.Configuration.AppFilesystem;
using Tiempitod.NET.Configuration.Notifications;
using Tiempitod.NET.Configuration.Server;
using Tiempitod.NET.Configuration.Session;
using Tiempitod.NET.Configuration.User;
using Tiempitod.NET.Notifications;
using Tiempitod.NET.Server;
#if LINUX
using Tiempitod.NET.Notifications.Linux;
#elif WINDOWS10_0_17763_0_OR_GREATER
using Tiempitod.NET.Notifications.Windows;
#endif
using Tiempitod.NET.Session;

var builder = Host.CreateApplicationBuilder(args);

IServiceProvider serviceProvider = builder.Services.BuildServiceProvider();
var loggerProvider =  serviceProvider.GetService<ILoggerFactory>();
ArgumentNullException.ThrowIfNull(loggerProvider);

// Add configuration services
IAppFilesystemPathProvider appFilesystemPathProvider = new AppFilesystemPathProvider(loggerProvider.CreateLogger<AppFilesystemPathProvider>());
builder.Services.AddSingleton(appFilesystemPathProvider);

IFileProvider userConfigFileProvider = new PhysicalFileProvider(appFilesystemPathProvider.UserConfigDirectoryPath);
builder.Services.AddKeyedSingleton(
    AppConfigConstants.UserConfigFileProviderKey,
    userConfigFileProvider);
builder.Services.AddKeyedSingleton(
    AppConfigConstants.UserConfigParserServiceKey,
    new ConfigParser(userConfigFileProvider.GetFileInfo(AppConfigConstants.UserConfigFileName).PhysicalPath)); // BUG: If two section names are equals throws an exception.

builder.Services.AddSingleton<UserConfigFileCreator>();
builder.Services.AddSingleton<IUserConfigReader, UserConfigReader>();
builder.Services.AddSingleton<IUserConfigWriter, UserConfigWriter>();
builder.Services.AddSingleton<ISessionConfigReader, SessionConfigReader>();
builder.Services.AddSingleton<ISessionConfigWriter, SessionConfigWriter>();

builder.Services.AddSingleton<UserConfigProvider>();
builder.Services.AddSingleton<SessionConfigProvider>();

builder.Services.AddSingleton<IUserConfigProvider>(sp => sp.GetService<UserConfigProvider>()!);
builder.Services.AddSingleton<ISessionConfigProvider>(sp => sp.GetService<SessionConfigProvider>()!);

// Load daemon config
builder.Configuration.AddIniFile(appFilesystemPathProvider.DaemonConfigFilePath, optional: false, reloadOnChange: true);
builder.Services.Configure<PipeConfig>(builder.Configuration.GetRequiredSection(key: PipeConfig.Pipe));
builder.Services.Configure<NotificationConfig>(builder.Configuration.GetSection(key: NotificationConfig.Notification));

// Add configuration dependencies.
PipeConfig pipeConfig = builder.Services.BuildServiceProvider().GetService<IOptions<PipeConfig>>()?.Value!;
builder.Services.AddTransient(_ => pipeConfig.GetEncoding());
var jsonSerializerOptions = new JsonSerializerOptions()
{
    TypeInfoResolver = IpcSerializerContext.Default
};
builder.Services.AddSingleton(jsonSerializerOptions);
builder.Services.AddTransient<IAsyncPacketHandler, PipePacketHandler>();
builder.Services.AddTransient<IPacketSerializer, PacketSerializer>();
builder.Services.AddTransient<IPacketDeserializer, PacketDeserializer>();

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
var serverPipe = new NamedPipeServerStream
(
    pipeConfig.PipeName,
    pipeConfig.PipeDirection,
    pipeConfig.PipeMaxInstances,
    PipeTransmissionMode.Byte,
    PipeOptions.Asynchronous
);
builder.Services.AddSingleton(serverPipe);

// Add stdout handler
TextWriter pipeStdOut = new StreamWriter(serverPipe);
builder.Services.AddSingleton(pipeStdOut);
var stdOutProcessor = new StandardOutMessageProcessor([], pipeStdOut);
builder.Services.AddSingleton(stdOutProcessor);
builder.Services.AddSingleton<IStandardOutSink>(stdOutProcessor);
builder.Services.AddSingleton<IStandardOutQueue>(stdOutProcessor);

// Add time provider.
builder.Services.AddSingleton(TimeProvider.System);
    
var sessionProgress = new Progress<Session>();
builder.Services.AddSingleton(sessionProgress);
builder.Services.AddSingleton<IProgress<Session>>(sessionProgress);
builder.Services.AddSingleton<ISessionStorage, SessionStorage>();
builder.Services.AddKeyedSingleton(typeof(TimeSpan), "TimingInterval", (_, _) => TimeSpan.FromSeconds(1));
builder.Services.AddSingleton<ISessionTimer, SessionTimer>();

builder.Services.AddSingleton<Server>();
builder.Services.AddSingleton<RequestOrchestrator>();
builder.Services.AddSingleton<SessionManager>();
builder.Services.AddSingleton<NotificationManager>();

builder.Services.AddSingleton<IServer>(sp => sp.GetService<Server>()!);
builder.Services.AddSingleton<ICommandHandlerFactory, CommandHandlerFactory>();
builder.Services.AddSingleton<ISessionManager>(sp => sp.GetService<SessionManager>()!);
builder.Services.AddSingleton<INotificationManager>(sp => sp.GetService<NotificationManager>()!);

builder.Services.AddSingleton<DaemonService>(sp => sp.GetService<UserConfigFileCreator>()!);
builder.Services.AddSingleton<DaemonService>(sp => sp.GetService<UserConfigProvider>()!);
builder.Services.AddSingleton<DaemonService>(sp => sp.GetService<SessionConfigProvider>()!);
builder.Services.AddSingleton<DaemonService>(sp => sp.GetService<Server>()!);
builder.Services.AddSingleton<DaemonService>(sp => sp.GetService<RequestOrchestrator>()!);
builder.Services.AddSingleton<DaemonService>(sp => sp.GetService<SessionManager>()!);
builder.Services.AddSingleton<DaemonService>(sp => sp.GetService<NotificationManager>()!);

builder.Services.AddHostedService<DaemonWorker>();

var host = builder.Build();
host.Run();
