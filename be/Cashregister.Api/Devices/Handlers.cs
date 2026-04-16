using System.Collections.Immutable;

using Cashregister.Api.Devices.Models;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Cashregister.Api.Devices;

internal static class Handlers
{
    public static async Task<Ok<ImmutableArray<DeviceDto>>> GetDevices(
        IPrinterDeviceCatalog deviceCatalog,
        FileDeviceTargetSelector targetSelector,
        CancellationToken cancellationToken
    )
    {
        var devices = await deviceCatalog.ListAsync(cancellationToken);
        var currentTarget = targetSelector.CurrentTarget;

        var deviceDtos = devices
            .Select(device => new DeviceDto(
                device.Id,
                device.Name,
                device.Target,
                device.Description,
                device.Target == currentTarget
            ))
            .ToImmutableArray();

        return TypedResults.Ok(deviceDtos);
    }

    public static async Task<Results<NotFound, NoContent>> SelectDevice(
        FileDeviceTargetSelector targetSelector,
        [FromRoute] string id,
        CancellationToken cancellationToken
    )
    {
        var selected = await targetSelector.SelectAsync(id, cancellationToken);

        if (!selected)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }
}