using Cashregister.Application.Pagination;
using Cashregister.Domain;

namespace Cashregister.Application.Orders.Models.Output;

public sealed class OrderListItem : IPageItem
{
    public required Identifier Id { get; init; }

    public required OrderNumber Number { get; init; }

    public required Cents Total { get; init; }

    public required TimeStamp Date { get; init; }
}
