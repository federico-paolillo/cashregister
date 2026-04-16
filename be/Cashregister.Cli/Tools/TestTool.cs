using Cashregister.Printmon.Devices;
using Cashregister.Printmon.Instructions.Layout;

namespace Cashregister.Printmon.Tools;

public sealed class TestTool(
    IDevice Printer
)
{
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var printProgram = new PrintProgramBuilder()
            .LineFeed()
            .Align(Alignment.Left)
            .FontSize(1)
            .PrintLine("Hello, world!")
            .FontSize(2)
            .Align(Alignment.Center)
            .PrintLine("Hello, world!")
            .Align(Alignment.Right)
            .FontSize(3)
            .PrintLine("Hello, world!")
            .Align(Alignment.Left)
            .FontSize(1)
            .BoldOn()
            .PrintLine("Emphasis")
            .BoldOff()
            .PrintLine("Emphasis")
            .DoubleStrikeOn()
            .PrintLine("Dstrike")
            .DoubleStrikeOff()
            .Build();

        await Printer.PrintAsync(printProgram);

        return 0;
    }
}