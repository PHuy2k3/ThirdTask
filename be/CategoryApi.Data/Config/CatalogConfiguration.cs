using CategoryApi.Data.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CategoryApi.Data.Mapping;

public sealed class CatalogConfiguration : IEntityTypeConfiguration<Catalog>
{
    public void Configure(EntityTypeBuilder<Catalog> b)
    {
        b.ToTable("Catalogs");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(220).IsRequired();
        b.Property(x => x.Price).HasColumnType("decimal(18,2)").HasDefaultValue(0);
        b.Property(x => x.ImageUrl).HasMaxLength(500);

        b.Property(x => x.IsActive).HasDefaultValue(true);

        b.Property(x => x.CreatedAt);
        b.Property(x => x.UpdatedAt);

        b.HasIndex(x => x.Code).IsUnique();
        b.HasIndex(x => new { x.CategoryId, x.Slug }).IsUnique();

        b.HasOne(x => x.Category)
         .WithMany(x => x.Catalogs)
         .HasForeignKey(x => x.CategoryId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
