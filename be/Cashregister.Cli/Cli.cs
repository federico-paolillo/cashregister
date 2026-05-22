using System.CommandLine;

using Cashregister.Printmon.Devices;
using Cashregister.Printmon.Tools;

using Microsoft.Extensions.DependencyInjection;

namespace Cashregister.Printmon;

public static class Cli
{
    public static RootCommand Create(IServiceCollection services)
    {
        var rootCommand = new RootCommand();

        rootCommand.Subcommands.Add(CreatePrintCommand(services));
        rootCommand.Subcommands.Add(CreateEmulateCommand(services));

        return rootCommand;
    }

    private static Command CreatePrintCommand(IServiceCollection services)
    {
        var deviceOption = new Option<string>("--device")
        {
            Required = true,
            DefaultValueFactory = (_) => "/dev/usb/lp0",
            Recursive = true
        };

        var printCommand = new Command("print")
        {
            deviceOption
        };

        var testCommand = new Command("test");

        testCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var device = parseResult.GetValue(deviceOption);

            using var svcProvider = services.BuildServiceProvider();
            using var scope = svcProvider.CreateScope();

            var targetStore = scope.ServiceProvider.GetRequiredService<FileDeviceTargetStore>();
            targetStore.Select(device!);

            var tool = scope.ServiceProvider.GetRequiredService<TestTool>();

            return await tool.ExecuteAsync(cancellationToken);
        });

        printCommand.Subcommands.Add(testCommand);

        return printCommand;
    }

    private static Command CreateEmulateCommand(IServiceCollection services)
    {
        var inputOption = new Option<string>("--input") { Required = true };

        var emulateCommand = new Command("emulate") { inputOption };

        emulateCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var input = parseResult.GetValue(inputOption)!;

            using var svcProvider = services.BuildServiceProvider();
            using var scope = svcProvider.CreateScope();

            var tool = scope.ServiceProvider.GetRequiredService<EmulateTool>();

            return await tool.ExecuteAsync(input, cancellationToken);
        });

        return emulateCommand;
    }
}