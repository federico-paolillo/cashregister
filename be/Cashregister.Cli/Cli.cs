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
        var orderOption = new Option<string>("--order")
        {
            Description = "Order identifier",
            Required = true
        };

        var command = new Command("print") { orderOption };

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            using var scope = svcProvider.CreateScope();

            var tool = scope.ServiceProvider.GetRequiredService<PrintTool>();

            var orderId = parseResult.GetRequiredValue(orderOption);

            return await tool.ExecuteAsync(orderId, cancellationToken);
        });

        return command;
    }
}
