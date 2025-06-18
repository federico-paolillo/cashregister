using Cashregister.Database.Entities;

using Microsoft.EntityFrameworkCore;

namespace Cashregister.Database;

public interface IApplicationDbContext
{
    DbSet<OrderEntity> Orders { get; }

    DbSet<ArticleEntity> Articles { get; }

    DbSet<OrderItemEntity> OrderItems { get; }
}