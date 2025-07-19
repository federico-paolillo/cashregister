using Cashregister.Application;
using Cashregister.Database.Entities;
using Cashregister.Factories;

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

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        // In Entity Framework, creating the Database Context begins the Unit of Work
        // Therefore this method is a no-op in this implementation

        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        // In Entity Framework, destroying the Database Context without saving changes discards the Unit of Work
        // Therefore this method is a no-op in this implementation

        return Task.CompletedTask;
    }
}