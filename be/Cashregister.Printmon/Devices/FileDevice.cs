using Cashregister.Factories;
using Cashregister.Printmon.Devices.Problems;
using Cashregister.Printmon.Encoders;

namespace Cashregister.Printmon.Devices;

public sealed class FileDevice(
    FileDeviceTargetStore targetStore,
    IEncoder<byte[]> encoder
) : IDevice
{
    public async Task<Result<Unit>> PrintAsync(PrintProgram printProgram)
    {
        var encodeResult = encoder.Encode(printProgram);

        if (encodeResult.NotOk)
        {
            return Result.Error(encodeResult.Error);
        }

        var currentTarget = targetStore.CurrentTarget;
        if (currentTarget is null)
        {
            return Result.Error(new NoSelectedFileDeviceTargetProblem());
        }

        await using var fileDeviceStream = new FileStream(currentTarget, FileMode.Open, FileAccess.Write, FileShare.Write, 4096, useAsync: true);

        await fileDeviceStream.WriteAsync(encodeResult.Value);

        return Result.Void();
    }
}