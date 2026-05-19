using System.Collections.Immutable;

using Cashregister.Domain;

namespace Cashregister.Application.Receipts.Models.Output;

/// <summary>
///     Projection containing the order data required to build a receipt print program.
/// </summary>
public sealed class OrderPrintData
{
    public required Identifier Id { get; init; }

    public required OrderNumber Number { get; init; }

    public required TimeStamp Date { get; init; }

    public required Cents Total { get; init; }

    public required ImmutableArray<OrderPrintDataItem> Items { get; init; }
}