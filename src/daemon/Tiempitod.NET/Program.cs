using System.Text;
using Tiempitod.NET;
using Tiempitod.NET.Commands.Handler;
using Tiempitod.NET.Commands.Server;
using Tiempitod.NET.Session;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTransient<Encoding, UTF8Encoding>();
builder.Services.AddTransient<IProgress<Session>, Progress<Session>>();

builder.Services.AddSingleton<CommandServer>();
builder.Services.AddSingleton<CommandHandler>();
builder.Services.AddSingleton<SessionManager>();

builder.Services.AddSingleton<ICommandServer>(sp => sp.GetService<CommandServer>()!);
builder.Services.AddSingleton<ICommandHandler>(sp => sp.GetService<CommandHandler>()!);
builder.Services.AddSingleton<ISessionManager>(sp => sp.GetService<SessionManager>()!);

builder.Services.AddSingleton<DaemonService>(sp => sp.GetService<CommandServer>()!);
builder.Services.AddSingleton<DaemonService>(sp => sp.GetService<CommandHandler>()!);
builder.Services.AddSingleton<DaemonService>(sp => sp.GetService<SessionManager>()!);

builder.Services.AddHostedService<DaemonWorker>();

var host = builder.Build();
host.Run();
