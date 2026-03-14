using Cashregister.Domain;

namespace Cashregister.Application.Pagination;

public interface IPageItem
{
    Identifier Id { get; }
}