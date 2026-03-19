using Cashregister.Printmon.Devices;
using Cashregister.Printmon.Instructions.Layout;

namespace Cashregister.Printmon.Tools;

public sealed class TestTool(
    IDevice Printer
)
{
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var printProgram = new PrintProgramBuilder()
            .Justify(Justification.Center)
            .Text("Hello, world!")
            .Build();

        await Printer.PrintAsync(printProgram);

        return 0;
    }
}