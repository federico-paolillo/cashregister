using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashRegister.Database.Entities;

public sealed class OrderEntity
{
    public required string Id { get; init; }
    
    public long RowId { get; private set; }
    
    public required long Date { get; init; }

    public required List<OrderItemEntity> Items { get; init; } = [];
    
    public sealed class OrderEntityTypeConfiguration : IEntityTypeConfiguration<OrderEntity>
    {
        public void Configure(EntityTypeBuilder<OrderEntity> builder)
        {
            builder.Property(p => p.Id)
                .IsRequired();

            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.Date)
                .IsRequired();

            builder.HasMany(p => p.Items)
                .WithOne()
                .HasForeignKey(p => p.OrderId);

            builder.Property(x => x.RowId)
                .HasColumnName("_rowid_")
                .IsRowVersion()
                .IsRequired();
        }
    }
}