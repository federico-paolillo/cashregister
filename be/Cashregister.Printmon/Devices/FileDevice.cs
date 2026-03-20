using Cashregister.Factories;
using Cashregister.Printmon.Encoders;

using Microsoft.Extensions.Options;

namespace Cashregister.Printmon.Devices;

public sealed class FileDevice(
    IOptions<FileDeviceSettings> Options,
    IEncoder<byte[]> Encoder
) : IDevice
{
  private FileDeviceSettings Settings => Options.Value;

  public async Task<Result<Unit>> PrintAsync(PrintProgram printProgram)
  {
    var encodeResult = Encoder.Encode(printProgram);

    if (encodeResult.NotOk)
    {
      return Result.Error(encodeResult.Error);
    }

    await using var fileDeviceStream = new FileStream(Settings.Target, FileMode.Open, FileAccess.Write, FileShare.Write, 4096, useAsync: true);

    await fileDeviceStream.WriteAsync(encodeResult.Value);

    return Result.Void();
  }
}