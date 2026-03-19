using System.CommandLine;

using Cashregister.Printmon.Tools;

using Microsoft.Extensions.DependencyInjection;

namespace Cashregister.Printmon;

public static class Cli
{
    public static RootCommand Create(IServiceProvider svcProvider)
    {
        var rootCommand = new RootCommand();

        rootCommand.Subcommands.Add(CreatePrintCommand(svcProvider));

        return rootCommand;
    }

    private static Command CreatePrintCommand(IServiceProvider svcProvider)
    {
        var printCommand = new Command("print");

        var testCommand = new Command("test");

        testCommand.SetAction(async (_, cancellationToken) =>
        {
            using var scope = svcProvider.CreateScope();

            var tool = scope.ServiceProvider.GetRequiredService<TestTool>();

            return await tool.ExecuteAsync(cancellationToken);
        });

        printCommand.Subcommands.Add(testCommand);

        return printCommand;
    }
}