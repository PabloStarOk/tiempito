using System.Text;
using Tiempitod.NET;
using Tiempitod.NET.Commands;
using Tiempitod.NET.Session;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTransient<Encoding, UTF8Encoding>();
builder.Services.AddTransient<ICommandListener, CommandListener>();
builder.Services.AddTransient<ICommandHandler, CommandHandler>();
builder.Services.AddTransient<IProgress<Session>, Progress<Session>>();
builder.Services.AddTransient<ISessionManager, SessionManager>();
builder.Services.AddHostedService<DaemonWorker>();

var host = builder.Build();
host.Run();
