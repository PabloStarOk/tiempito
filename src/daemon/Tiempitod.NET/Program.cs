using Tiempitod.NET;
using Tiempitod.NET.Session;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTransient<IProgress<SessionProgress>, Progress<SessionProgress>>();
builder.Services.AddTransient<ISessionManager, SessionManager>();
builder.Services.AddHostedService<DaemonWorker>();

var host = builder.Build();
host.Run();
