using Cashregister.Api.ReceiptModes.Models;
using Cashregister.Application.Receipts.Models;
using Cashregister.Application.Receipts.Services;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Cashregister.Api.ReceiptModes;

internal static class Handlers
{
    public static Ok<ReceiptModeDto> GetReceiptMode(ReceiptModeStore receiptModeStore)
    {
        return TypedResults.Ok(new ReceiptModeDto(Format(receiptModeStore.Current)));
    }

    public static Results<BadRequest, NoContent> SelectReceiptMode(
        ReceiptModeStore receiptModeStore,
        [FromRoute] string mode
    )
    {
        if (!TryParse(mode, out var receiptMode))
        {
            return TypedResults.BadRequest();
        }

        receiptModeStore.Select(receiptMode);

        return TypedResults.NoContent();
    }

    private static bool TryParse(string mode, out ReceiptMode receiptMode)
    {
        switch (mode)
        {
            case "normal":
                receiptMode = ReceiptMode.Normal;
                return true;
            case "detail":
                receiptMode = ReceiptMode.Detail;
                return true;
            default:
                receiptMode = ReceiptMode.Normal;
                return false;
        }
    }

    private static string Format(ReceiptMode receiptMode)
    {
        return receiptMode switch
        {
            ReceiptMode.Normal => "normal",
            ReceiptMode.Detail => "detail",
            _ => throw new ArgumentOutOfRangeException(nameof(receiptMode), receiptMode, null)
        };
    }
}