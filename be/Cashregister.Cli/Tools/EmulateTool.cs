using Cashregister.Printmon.Emulator;

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
            await Console.Error.WriteLineAsync($"Emulation failed: {result.Error.GetType().Name}");
            return 1;
        }

        var markdown = markdownRenderer.Render(result.Value[^1].Receipt);
        Console.Write(markdown);

        return 0;
    }
}
