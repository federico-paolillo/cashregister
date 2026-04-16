using Cashregister.Printmon.Devices;

namespace Cashregister.Api.Devices;

public sealed class FileDeviceTargetSelector(
    IPrinterDeviceCatalog deviceCatalog,
    FileDeviceTargetStore targetStore
)
{
    public string CurrentTarget => targetStore.CurrentTarget;

    public async Task<bool> SelectAsync(string id, CancellationToken cancellationToken)
    {
        var devices = await deviceCatalog.ListAsync(cancellationToken);
        var selectedDevice = devices.FirstOrDefault(device => device.Id == id);

        if (selectedDevice is null)
        {
            return false;
        }

        targetStore.Select(selectedDevice.Target);

        return true;
    }
}