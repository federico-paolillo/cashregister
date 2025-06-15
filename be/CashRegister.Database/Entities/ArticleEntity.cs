using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashRegister.Database.Entities;

public sealed class ArticleEntity
{
    public required string Id { get; init; }
    
    public required string Description { get; init; }
    
    public required long Price { get; init; }
    
    public sealed class ArticleEntityTypeConfiguration : IEntityTypeConfiguration<ArticleEntity>
    {
        public void Configure(EntityTypeBuilder<ArticleEntity> builder)
        {
            builder.Property(p => p.Id)
                .IsRequired();

            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.Price)
                .IsRequired();

            builder.Property(p => p.Description)
                .IsRequired();

            builder.HasMany<OrderItemEntity>()
                .WithOne()
                .HasForeignKey(p => p.ArticleId);
        }
    }
}