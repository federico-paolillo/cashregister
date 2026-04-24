namespace Cashregister.Application.Devices.Models;

public sealed record PrinterDevice(
    string Id,
    string Name,
    string Target,
    string? Description
);