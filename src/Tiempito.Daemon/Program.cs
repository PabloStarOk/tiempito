using Tiempito.Daemon.Application;
using Tiempito.Daemon.Infrastructure;
using Tiempito.Daemon.Server;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddServer(builder.Configuration);
var host = builder.Build();
host.Run();
