using Cashregister.Application.Pagination;
using Cashregister.Domain;

namespace Cashregister.Application.Orders.Models.Output;

public sealed class OrderListItem : IPageItem
{
    public required Identifier Id { get; init; }

    public required long Number { get; init; }

    public required long Total { get; init; }

    public required long Date { get; init; }
}
