using Cashregister.Application;
using Cashregister.Database.Entities;

using Microsoft.EntityFrameworkCore;

namespace Cashregister.Database;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options
) : DbContext(options), IApplicationDbContext, IUnitOfWork
{
    public DbSet<OrderEntity> Orders { get; set; }

    public DbSet<ArticleEntity> Articles { get; set; }

    public DbSet<OrderItemEntity> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}