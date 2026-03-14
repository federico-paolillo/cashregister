using Cashregister.Domain;

namespace Cashregister.Tests.Integration.Orders;

public sealed class OrderTests
{
    [Fact]
    public void Total_ShouldReturnSumOfItemPricesTimesQuantities()
    {
        var order = new Order
        {
            Id = Identifier.New(),
            Number = OrderNumber.From(1),
            Date = TimeStamp.Now(),
            Items =
            [
                new Item
                {
                    Id = Identifier.New(),
                    Article = Identifier.New(),
                    Description = "Article A",
                    Price = Cents.From(500),
                    Quantity = 2
                },
                new Item
                {
                    Id = Identifier.New(),
                    Article = Identifier.New(),
                    Description = "Article B",
                    Price = Cents.From(300),
                    Quantity = 3
                }
            ]
        };

        var total = order.Total();

        // 500 * 2 + 300 * 3 = 1000 + 900 = 1900
        Assert.Equal(Cents.From(1900), total);
    }

    [Fact]
    public void Total_WithNoItems_ShouldReturnZero()
    {
        var order = new Order
        {
            Id = Identifier.New(),
            Number = OrderNumber.From(1),
            Date = TimeStamp.Now(),
            Items = []
        };

        var total = order.Total();

        Assert.Equal(Cents.From(0), total);
    }
}