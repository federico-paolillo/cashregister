using Cashregister.Application.Receipts.Services;
using Cashregister.Domain;
using Cashregister.Factories;
using Cashregister.Printmon.Devices;

namespace Cashregister.Application.Receipts.Handlers.Defaults;

/// <summary>
///     Default receipt print handler that builds and sends receipt print programs to the configured device.
/// </summary>
public sealed class PrintReceiptHandler(
    IReceiptPrintProgramService receiptPrintProgramService,
    IDevice device
) : IPrintReceiptHandler
{
    public async Task<Result<Unit>> ExecuteAsync(Identifier orderId)
    {
        ArgumentNullException.ThrowIfNull(orderId);

        var printProgramResult = await receiptPrintProgramService.BuildAsync(orderId);

        if (printProgramResult.NotOk)
        {
            return Result.Error(printProgramResult.Error);
        }

        return await device.PrintAsync(printProgramResult.Value);
    }
}