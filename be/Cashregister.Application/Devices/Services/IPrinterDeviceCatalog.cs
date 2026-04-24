using Cashregister.Application.Devices.Models;

namespace Cashregister.Application.Devices.Services;

public interface IPrinterDeviceCatalog
{
    Task<IReadOnlyList<PrinterDevice>> ListAsync(CancellationToken cancellationToken);
}