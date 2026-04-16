namespace Cashregister.Api.Devices;

public interface IPrinterDeviceCatalog
{
    Task<IReadOnlyList<PrinterDevice>> ListAsync(CancellationToken cancellationToken);
}