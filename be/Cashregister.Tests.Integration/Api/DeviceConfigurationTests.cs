using Cashregister.Printmon;
using Cashregister.Printmon.Devices;
using Cashregister.Printmon.Emulator.Device;
using Cashregister.Printmon.Instructions.Layout;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Api;

public sealed class DeviceConfigurationTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task Application_UsesMarkdownDevice_InDevelopment()
    {
        var rootFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(rootFolder);

        try
        {
            await PrepareEnvironmentAsync(
                services =>
                {
                    services.Configure<MarkdownDeviceSettings>(options => options.RootFolder = rootFolder);
                }
            );

            using var scope = NewServiceScope();

            var device = scope.ServiceProvider.GetRequiredService<IDevice>();
            var printResult = await device.PrintAsync(new PrintProgramBuilder()
                .Align(Alignment.Center)
                .BoldOn()
                .PrintLine("DEV RECEIPT")
                .BoldOff()
                .Build());

            Assert.IsType<MarkdownDevice>(device);
            Assert.True(printResult.Ok);

            var file = Directory.GetFiles(rootFolder).Single();
            var markdown = await File.ReadAllTextAsync(file);

            Assert.Contains("<p align=\"center\">**DEV RECEIPT**</p>", markdown, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(rootFolder, recursive: true);
        }
    }

    [Fact]
    public async Task Application_UsesFileDevice_OutsideDevelopment()
    {
        await PrepareEnvironmentAsync(Environments.Production);

        using var scope = NewServiceScope();

        var device = scope.ServiceProvider.GetRequiredService<IDevice>();

        Assert.IsType<FileDevice>(device);
    }
}