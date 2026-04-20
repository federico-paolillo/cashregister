using Cashregister.Factories;
using Cashregister.Printmon.Devices.Problems;
using Cashregister.Printmon.Encoders;

using Microsoft.Extensions.Options;

namespace Cashregister.Printmon.Devices;

/// <summary>
///     A development-only device that emits a <see cref="PrintProgram" /> to disk to a random file relative to a root
///     folder
/// </summary>
public sealed class RandomFileDevice(
  IOptions<RandomFileDeviceSettings> options,
  IEncoder<byte[]> encoder
) : IDevice
{
    public async Task<Result<Unit>> PrintAsync(PrintProgram printProgram)
    {
        ArgumentNullException.ThrowIfNull(printProgram);

        var randomFileName = Path.GetRandomFileName();

        var destinationPath = Path.Combine(options.Value.RootFolder, randomFileName);

        var encodeResult = encoder.Encode(printProgram);

        if (encodeResult.NotOk)
        {
            return Result.Error(encodeResult.Error);
        }

        return await EmitBytesAsync(destinationPath, encodeResult.Value);
    }

    private static async Task<Result<Unit>> EmitBytesAsync(
      string destinationPath,
      byte[] printProgram)
    {
        try
        {
            await using var fileDeviceStream = new FileStream(destinationPath, FileMode.Open, FileAccess.Write, FileShare.Write, 4096, useAsync: true);

            await fileDeviceStream.WriteAsync(printProgram);

            return Result.Void();
        }
        catch (IOException ioEx)
        {
            return Result.Error(new DeviceIoProblem(ioEx));
        }
    }
}