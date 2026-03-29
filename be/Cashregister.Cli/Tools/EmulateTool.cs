using Cashregister.Printmon.Emulator;
using Cashregister.Printmon.Emulator.Problems;

namespace Cashregister.Printmon.Tools;

public sealed class EmulateTool(
    IPrinterEmulator printerEmulator,
    IMarkdownRenderer markdownRenderer)
{
    public async Task<int> ExecuteAsync(string inputPath, CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(inputPath, cancellationToken);
        var result = printerEmulator.Emulate(bytes);

        if (result.NotOk)
        {
            switch (result.Error)
            {
                case UnrecognizedBytesProblem u:
                    await Console.Error.WriteLineAsync($"At offset '{u.Offset}' within '{u.Context}'");
                    break;
                default:
                    await Console.Error.WriteLineAsync($"Emulation failed: {result.Error.GetType().Name}");
                    break;
            }

            return 1;
        }

        var markdown = markdownRenderer.Render(result.Value[^1].Receipt);
        Console.Write(markdown);

        return 0;
    }
}
