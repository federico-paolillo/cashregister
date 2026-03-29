using Cashregister.Printmon.Emulator;

namespace Cashregister.Printmon.Tools;

public sealed class EmulateTool(
    IProgramExecutor programExecutor,
    IMarkdownRenderer markdownRenderer)
{
    public async Task<int> ExecuteAsync(string inputPath, CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(inputPath, cancellationToken);
        var document = programExecutor.Execute(bytes);
        var markdown = markdownRenderer.Render(document);

        Console.Write(markdown);

        return 0;
    }
}
