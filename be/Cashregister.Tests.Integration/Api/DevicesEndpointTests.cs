using System.Net;

using Cashregister.Api.Devices;
using Cashregister.Api.Devices.Models;
using Cashregister.Printmon.Devices;

using Microsoft.Extensions.DependencyInjection.Extensions;

using Xunit.Abstractions;

namespace Cashregister.Tests.Integration.Api;

public sealed class DevicesEndpointTests(
    ITestOutputHelper testOutputHelper
) : IntegrationTest(testOutputHelper)
{
    [Fact]
    public async Task GetDevices_ReturnsDevicesWithSelectedDevice()
    {
        await PrepareEnvironmentAsync(services => ConfigureDevices(
            services,
            "/dev/usb/lp1",
            [
                new PrinterDevice("printer-0", "Receipt Printer A", "/dev/usb/lp0", "Front counter"),
                new PrinterDevice("printer-1", "Receipt Printer B", "/dev/usb/lp1", "Back counter")
            ]
        ));

        using var httpClient = CreateHttpClient();

        var response = await httpClient.GetAsync("/devices");
        var devices = await response.Content.ReadFromJsonAsync<DeviceDto[]>();

        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(devices);
        Assert.Equal(2, devices.Length);
        Assert.False(devices[0].Selected);
        Assert.True(devices[1].Selected);
        Assert.Equal("Receipt Printer B", devices[1].Name);
        Assert.Equal("/dev/usb/lp1", devices[1].Target);
    }

    [Fact]
    public async Task SelectDevice_ChangesSelectedDevice()
    {
        await PrepareEnvironmentAsync(services => ConfigureDevices(
            services,
            "/dev/usb/lp0",
            [
                new PrinterDevice("printer-0", "Receipt Printer A", "/dev/usb/lp0", null),
                new PrinterDevice("printer-1", "Receipt Printer B", "/dev/usb/lp1", null)
            ]
        ));

        using var httpClient = CreateHttpClient();

        var selectResponse = await httpClient.PostAsync("/devices/printer-1", null);
        var devicesResponse = await httpClient.GetAsync("/devices");
        var devices = await devicesResponse.Content.ReadFromJsonAsync<DeviceDto[]>();

        Assert.Equal(HttpStatusCode.NoContent, selectResponse.StatusCode);
        Assert.True(devicesResponse.IsSuccessStatusCode);
        Assert.NotNull(devices);
        Assert.False(devices.Single(device => device.Id == "printer-0").Selected);
        Assert.True(devices.Single(device => device.Id == "printer-1").Selected);
    }

    [Fact]
    public async Task SelectDevice_ReturnsNotFound_WhenDeviceDoesNotExist()
    {
        await PrepareEnvironmentAsync(services => ConfigureDevices(
            services,
            "/dev/usb/lp0",
            [
                new PrinterDevice("printer-0", "Receipt Printer A", "/dev/usb/lp0", null)
            ]
        ));

        using var httpClient = CreateHttpClient();

        var response = await httpClient.PostAsync("/devices/missing", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ApiPrefixedDevicesRoute_ReturnsNotFound()
    {
        await PrepareEnvironmentAsync(services => ConfigureDevices(
            services,
            "/dev/usb/lp0",
            [
                new PrinterDevice("printer-0", "Receipt Printer A", "/dev/usb/lp0", null)
            ]
        ));

        using var httpClient = CreateHttpClient();

        var response = await httpClient.GetAsync("/api/devices");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static void ConfigureDevices(
        IServiceCollection services,
        string target,
        IReadOnlyList<PrinterDevice> devices
    )
    {
        services.RemoveAll<IPrinterDeviceCatalog>();
        services.Configure<FileDeviceSettings>(options => options.Target = target);
        services.AddSingleton<IPrinterDeviceCatalog>(_ => new StubPrinterDeviceCatalog(devices));
    }

    private sealed class StubPrinterDeviceCatalog(
        IReadOnlyList<PrinterDevice> devices
    ) : IPrinterDeviceCatalog
    {
        public Task<IReadOnlyList<PrinterDevice>> ListAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(devices);
        }
    }
}