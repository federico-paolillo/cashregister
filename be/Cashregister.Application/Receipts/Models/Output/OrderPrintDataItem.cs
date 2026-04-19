namespace Cashregister.Application.Receipts.Models.Output;

/// <summary>
///     Projection containing the item data required to print one receipt line.
/// </summary>
public sealed class OrderPrintDataItem
{
    public required string Description { get; init; }

    public required uint Quantity { get; init; }
}