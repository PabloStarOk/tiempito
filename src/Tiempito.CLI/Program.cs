using Microsoft.Extensions.DependencyInjection;

using Tiempito.CLI;

var app = new CliApp(new ServiceCollection());
return await app.RunAsync(args);
