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
            .Justify(Justification.Left)
            .FontSize(1)
            .PrintLine("Hello, world!")
            .FontSize(2)
            .Justify(Justification.Center)
            .PrintLine("Hello, world!")
            .Justify(Justification.Right)
            .FontSize(3)
            .PrintLine("Hello, world!")
            .Justify(Justification.Left)
            .EmphasizeOn()
            .PrintLine("Emphasis")
            .EmphasizeOff()
            .PrintLine("Emphasis")
            .Build();

        await Printer.PrintAsync(printProgram);

        return 0;
    }
}