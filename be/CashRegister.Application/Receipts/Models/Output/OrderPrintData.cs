using Cashregister.Domain;

namespace Cashregister.Application.Receipts.Models.Output;

public sealed class OrderPrintData
{
    public required Identifier Id { get; init; }

    public required Cents Total { get; init; }
}