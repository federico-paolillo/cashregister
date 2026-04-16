namespace Cashregister.Api.Devices.Models;

public sealed record DeviceDto(
    string Id,
    string Name,
    string Target,
    string? Description,
    bool Selected
);