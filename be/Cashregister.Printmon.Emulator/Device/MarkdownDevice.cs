using Cashregister.Factories;
using Cashregister.Printmon.Devices;
using Cashregister.Printmon.Devices.Problems;
using Cashregister.Printmon.Encoders;

using Microsoft.Extensions.Options;

namespace Cashregister.Printmon.Emulator.Device;

/// <summary>
///     Development-only printer device that renders ESC/POS programs to markdown files through the emulator pipeline.
/// </summary>
public sealed class MarkdownDevice(
    IOptions<MarkdownDeviceSettings> options,
    IEncoder<byte[]> encoder,
    IPrinterEmulator printerEmulator,
    IMarkdownRenderer markdownRenderer
) : IDevice
{
    public async Task<Result<Unit>> PrintAsync(PrintProgram printProgram)
    {
        ArgumentNullException.ThrowIfNull(printProgram);

        var encodeResult = encoder.Encode(printProgram);
        if (encodeResult.NotOk)
        {
            return Result.Error(encodeResult.Error);
        }

        var emulateResult = printerEmulator.Emulate(encodeResult.Value);
        if (emulateResult.NotOk)
        {
            return Result.Error(emulateResult.Error);
        }

        var markdown = markdownRenderer.Render(emulateResult.Value[^1].Receipt);
        var destinationPath = Path.Combine(options.Value.RootFolder, BuildFileName());

        return await WriteMarkdownAsync(destinationPath, markdown);
    }

    private static string BuildFileName()
    {
        return $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Path.GetRandomFileName()}";
    }

    private static async Task<Result<Unit>> WriteMarkdownAsync(string destinationPath, string markdown)
    {
        try
        {
            await File.WriteAllTextAsync(destinationPath, markdown);

            return Result.Void();
        }
        catch (IOException ioEx)
        {
            return Result.Error(new DeviceIoProblem(ioEx));
        }
    }
}