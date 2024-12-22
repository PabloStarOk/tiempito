using Tiempitod.NET;
using Tiempitod.NET.Session;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTransient<IProgress<Session>, Progress<Session>>();
builder.Services.AddTransient<ISessionManager, SessionManager>();
builder.Services.AddHostedService<DaemonWorker>();

var host = builder.Build();
host.Run();
