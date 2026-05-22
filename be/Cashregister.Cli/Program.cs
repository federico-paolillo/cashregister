using Cashregister.Printmon;
using Cashregister.Printmon.Devices;
using Cashregister.Printmon.Emulator;
using Cashregister.Printmon.Encoders;
using Cashregister.Printmon.Tools;

using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddScoped<IDevice, FileDevice>();
services.AddScoped<IEncoder<byte[]>, BinaryEncoder>();
services.AddSingleton<FileDeviceTargetStore>();

services.AddScoped<TestTool>();

services.AddScoped<IInstructionDecoder, InstructionDecoder>();
services.AddScoped<IInstructionExecutor, InstructionExecutor>();
services.AddScoped<IPrinterEmulator, PrinterEmulator>();
services.AddScoped<IMarkdownRenderer, MarkdownRenderer>();
services.AddScoped<EmulateTool>();

var rootCommand = Cli.Create(services);

var parseResult = rootCommand.Parse(args);

var exitCode = await parseResult.InvokeAsync();

return exitCode;