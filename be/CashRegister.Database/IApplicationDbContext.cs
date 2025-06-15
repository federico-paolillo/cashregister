using CashRegister.Database.Entities;

using Microsoft.EntityFrameworkCore;

namespace CashRegister.Database;

public interface IApplicationDbContext
{
    DbSet<OrderEntity> Orders { get; }

    DbSet<ArticleEntity> Articles { get; }

    DbSet<OrderItemEntity> OrderItems { get; }
}