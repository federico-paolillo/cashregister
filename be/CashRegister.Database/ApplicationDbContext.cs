using CashRegister.Application;
using CashRegister.Database.Entities;
using CashRegister.Domain;

using Microsoft.EntityFrameworkCore;

namespace CashRegister.Database;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options
) : DbContext(options), IApplicationDbContext, IUnitOfWork
{
    public DbSet<OrderEntity> Orders { get; set; }

    public DbSet<ArticleEntity> Articles { get; set; }

    public DbSet<OrderItemEntity> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}