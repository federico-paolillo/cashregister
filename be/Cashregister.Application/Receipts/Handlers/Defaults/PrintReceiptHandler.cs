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

        var printProgramsResult = await receiptPrintProgramService.BuildAsync(orderId);

        if (printProgramsResult.NotOk)
        {
            return Result.Error(printProgramsResult.Error);
        }

        foreach (var printProgram in printProgramsResult.Value)
        {
            var printResult = await device.PrintAsync(printProgram);

            if (printResult.NotOk)
            {
                return printResult;
            }
        }

        return Result.Void();
    }
}