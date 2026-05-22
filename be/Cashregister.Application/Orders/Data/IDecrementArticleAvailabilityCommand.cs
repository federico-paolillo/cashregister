using Cashregister.Application.Orders.Models.Input;

namespace Cashregister.Application.Orders.Data;

/// <summary>
/// Decrements manually maintained article availability for placed order items.
/// </summary>
public interface IDecrementArticleAvailabilityCommand
{
    Task DecrementAsync(IEnumerable<OrderRequestItem> items);
}