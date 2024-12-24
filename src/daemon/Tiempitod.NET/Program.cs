using System.Text;
using Tiempitod.NET;
using Tiempitod.NET.Commands;
using Tiempitod.NET.Session;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTransient<Encoding, UTF8Encoding>();
builder.Services.AddTransient<IProgress<Session>, Progress<Session>>();

builder.Services.AddSingleton<CommandListener>();
builder.Services.AddSingleton<CommandHandler>();
builder.Services.AddSingleton<SessionManager>();

builder.Services.AddSingleton<ICommandListener>(sp => sp.GetService<CommandListener>()!);
builder.Services.AddSingleton<ICommandHandler>(sp => sp.GetService<CommandHandler>()!);
builder.Services.AddSingleton<ISessionManager>(sp => sp.GetService<SessionManager>()!);

builder.Services.AddSingleton<DaemonService>(sp => sp.GetService<CommandListener>()!);
builder.Services.AddSingleton<DaemonService>(sp => sp.GetService<CommandHandler>()!);
builder.Services.AddSingleton<DaemonService>(sp => sp.GetService<SessionManager>()!);

builder.Services.AddHostedService<DaemonWorker>();

var host = builder.Build();
host.Run();
