namespace Cashregister.Api.Devices;

public sealed record PrinterDevice(
    string Id,
    string Name,
    string Target,
    string? Description
);