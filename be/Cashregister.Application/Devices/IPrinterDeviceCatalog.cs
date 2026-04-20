namespace Cashregister.Application.Devices;

public interface IPrinterDeviceCatalog
{
    Task<IReadOnlyList<PrinterDevice>> ListAsync(CancellationToken cancellationToken);
}