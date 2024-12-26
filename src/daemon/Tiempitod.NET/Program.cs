using Microsoft.Extensions.Options;
using System.IO.Pipes;
using System.Text;
using Tiempitod.NET;
using Tiempitod.NET.Commands.Handler;
using Tiempitod.NET.Commands.Server;
using Tiempitod.NET.Configuration;
using Tiempitod.NET.Notifications;
#if LINUX
using Tiempitod.NET.Notifications.Linux;
#elif WINDOWS10_0_17763_0_OR_GREATER
using Tiempitod.NET.Notifications.Windows;
#endif
using Tiempitod.NET.Session;

var builder = Host.CreateApplicationBuilder(args);

// Load daemon config
builder.Configuration.AddIniFile(path: "tiempitod.conf", optional: false, reloadOnChange: true);
builder.Services.Configure<PipeConfig>(builder.Configuration.GetRequiredSection(key: PipeConfig.Pipe));
builder.Services.Configure<NotificationConfig>(builder.Configuration.GetSection(key: NotificationConfig.Notification));

PipeConfig pipeConfig = builder.Services.BuildServiceProvider().GetService<IOptions<PipeConfig>>()?.Value!;

// Add configuration dependencies.
builder.Services.AddTransient<Encoding>(_ => Activator.CreateInstance(pipeConfig.GetEncodingType()) as Encoding ?? Encoding.UTF8);
builder.Services.AddSingleton(new NamedPipeServerStream(
    pipeConfig.PipeName,
    pipeConfig.PipeDirection,
    pipeConfig.PipeMaxInstances));

builder.Services.AddTransient<IProgress<Session>, Progress<Session>>();
builder.Services.AddTransient<IAsyncMessageHandler, PipeMessageHandler>();

// Add system notification
#if LINUX
if (OperatingSystem.IsLinux())
    builder.Services.AddTransient<INotificationHandler, LinuxNotificationHandler>();
#elif WINDOWS10_0_17763_0_OR_GREATER
if (OperatingSystem.IsWindowsVersionAtLeast(10,0,10240))
    builder.Services.AddTransient<INotificationHandler, WindowsNotificationHandler>();
#endif

builder.Services.AddSingleton<CommandServer>();
builder.Services.AddSingleton<CommandHandler>();
builder.Services.AddSingleton<SessionManager>();
builder.Services.AddSingleton<NotificationManager>();

builder.Services.AddSingleton<ICommandServer>(sp => sp.GetService<CommandServer>()!);
builder.Services.AddSingleton<ICommandHandler>(sp => sp.GetService<CommandHandler>()!);
builder.Services.AddSingleton<ISessionManager>(sp => sp.GetService<SessionManager>()!);
builder.Services.AddSingleton<INotificationManager>(sp => sp.GetService<NotificationManager>()!);

builder.Services.AddSingleton<DaemonService>(sp => sp.GetService<CommandServer>()!);
builder.Services.AddSingleton<DaemonService>(sp => sp.GetService<CommandHandler>()!);
builder.Services.AddSingleton<DaemonService>(sp => sp.GetService<SessionManager>()!);
builder.Services.AddSingleton<DaemonService>(sp => sp.GetService<NotificationManager>()!);


builder.Services.AddHostedService<DaemonWorker>();

var host = builder.Build();
host.Run();
