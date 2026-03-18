using Cashregister.Printmon;
using Cashregister.Printmon.Tools;

using Microsoft.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection();

serviceCollection.AddScoped<PrintTool>();

var svcProvider = serviceCollection.BuildServiceProvider();

var rootCommand = Cli.Create(svcProvider);

var parseResult = rootCommand.Parse(args);

var exitCode = await parseResult.InvokeAsync();

return exitCode;
