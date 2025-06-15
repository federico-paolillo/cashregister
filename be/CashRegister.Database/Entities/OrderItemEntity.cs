using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashRegister.Database.Entities;

public sealed class OrderItemEntity
{
    public required string Id { get; init; }
    
    public required string OrderId { get; init; }
    
    public required string ArticleId { get; init; }
    
    public required string Description { get; init; }
    
    public required long Price { get; init; }
    
    public sealed class OrderItemEntityTypeConfiguration : IEntityTypeConfiguration<OrderItemEntity>
    {
        public void Configure(EntityTypeBuilder<OrderItemEntity> builder)
        {
            builder.Property(p => p.Id)
                .IsRequired();

            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.Price)
                .IsRequired();

            builder.Property(p => p.Description)
                .IsRequired();

            builder.HasOne<OrderEntity>()
                .WithMany()
                .HasForeignKey(p => p.OrderId);
            
            builder.HasOne<ArticleEntity>()
                .WithMany()
                .HasForeignKey(p => p.ArticleId);
        }
    }
}