using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cashregister.Database.Entities;

public sealed class ArticleEntity : IIdentifiableEntity
{
    public required string Description { get; set; }

    public required long Price { get; set; }

    public required bool Retired { get; set; }

    public required string Id { get; init; }

    public sealed class ArticleEntityTypeConfiguration : IEntityTypeConfiguration<ArticleEntity>
    {
        public void Configure(EntityTypeBuilder<ArticleEntity> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.HasQueryFilter(x => x.Retired == false);

            builder.Property(p => p.Id)
                .IsRequired();

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Price)
                .IsRequired();

            builder.Property(p => p.Description)
                .IsRequired();

            builder.Property(p => p.Retired)
                .IsRequired();

            builder.HasMany<OrderItemEntity>()
                .WithOne()
                .HasForeignKey(p => p.ArticleId);
        }
    }
}