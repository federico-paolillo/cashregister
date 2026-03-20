using Cashregister.Printmon;
using Cashregister.Printmon.Devices;
using Cashregister.Printmon.Encoders;
using Cashregister.Printmon.Tools;

using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddScoped<IDevice, FileDevice>();
services.AddScoped<IEncoder<byte[]>, BinaryEncoder>();

services.AddScoped<TestTool>();

var rootCommand = Cli.Create(services);

var parseResult = rootCommand.Parse(args);

var exitCode = await parseResult.InvokeAsync();

return exitCode;