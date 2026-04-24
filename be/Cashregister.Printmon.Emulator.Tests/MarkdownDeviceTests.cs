using Cashregister.Printmon.Devices.Problems;
using Cashregister.Printmon.Emulator.Device;
using Cashregister.Printmon.Encoders;
using Cashregister.Printmon.Instructions.Layout;

using Microsoft.Extensions.Options;

namespace Cashregister.Printmon.Emulator.Tests;

public sealed class MarkdownDeviceTests
{
    [Fact]
    public async Task PrintAsync_WritesRenderedMarkdownToTimestampedFile()
    {
        var rootFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(rootFolder);

        try
        {
            var device = CreateDevice(rootFolder);
            var printProgram = new PrintProgramBuilder()
                .Align(Alignment.Center)
                .BoldOn()
                .PrintLine("DEV RECEIPT")
                .BoldOff()
                .Build();

            var printResult = await device.PrintAsync(printProgram);

            Assert.True(printResult.Ok);

            var file = Directory.GetFiles(rootFolder).Single();
            var markdown = await File.ReadAllTextAsync(file);

            Assert.Matches(@"^\d{13}_.+", Path.GetFileName(file));
            Assert.Contains("<p align=\"center\">**DEV RECEIPT**</p>", markdown, StringComparison.Ordinal);
            Assert.Contains("---", markdown, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(rootFolder, recursive: true);
        }
    }

    [Fact]
    public async Task PrintAsync_ReturnsDeviceIoProblem_WhenRootFolderDoesNotExist()
    {
        var rootFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "missing");
        var device = CreateDevice(rootFolder);
        var printProgram = new PrintProgramBuilder()
            .PrintLine("DEV RECEIPT")
            .Build();

        var printResult = await device.PrintAsync(printProgram);

        Assert.True(printResult.NotOk);
        Assert.IsType<DeviceIoProblem>(printResult.Error);
    }

    private static MarkdownDevice CreateDevice(string rootFolder)
    {
        return new MarkdownDevice(
            Options.Create(new MarkdownDeviceSettings { RootFolder = rootFolder }),
            new BinaryEncoder(),
            new PrinterEmulator(new InstructionDecoder(), new InstructionExecutor()),
            new MarkdownRenderer()
        );
    }
}