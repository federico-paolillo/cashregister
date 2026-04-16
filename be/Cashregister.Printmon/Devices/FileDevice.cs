using Cashregister.Factories;
using Cashregister.Printmon.Encoders;

namespace Cashregister.Printmon.Devices;

public sealed class FileDevice(
    FileDeviceTargetStore TargetStore,
    IEncoder<byte[]> Encoder
) : IDevice
{
    public async Task<Result<Unit>> PrintAsync(PrintProgram printProgram)
    {
        var encodeResult = Encoder.Encode(printProgram);

        if (encodeResult.NotOk)
        {
            return Result.Error(encodeResult.Error);
        }

        await using var fileDeviceStream = new FileStream(TargetStore.CurrentTarget, FileMode.Open, FileAccess.Write, FileShare.Write, 4096, useAsync: true);

        await fileDeviceStream.WriteAsync(encodeResult.Value);

        return Result.Void();
    }
}