using Cashregister.Printmon.Devices;
using Cashregister.Printmon.Devices.Problems;
using Cashregister.Printmon.Encoders;

namespace Cashregister.Printmon.Tests.Devices;

public sealed class FileDeviceTests
{
    [Fact]
    public async Task PrintAsync_ReturnsFailure_WhenNoTargetIsSelected()
    {
        var fileDevice = new FileDevice(new FileDeviceTargetStore(), new BinaryEncoder());

        var result = await fileDevice.PrintAsync(new PrintProgramBuilder()
            .PrintLine("NO TARGET")
            .Build());

        Assert.True(result.NotOk);
        Assert.IsType<NoSelectedFileDeviceTargetProblem>(result.Error);
    }
}