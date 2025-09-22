using CategoryApi.Data.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CategoryApi.Data.Mapping
{
    public sealed class CategoryConfiguration: IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> b)
        {
            b.ToTable("Categories");
            b.HasKey(x => x.Id);

            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.Property(x => x.Slug).HasMaxLength(220).IsRequired();
            b.Property(x => x.SortOrder).HasDefaultValue(0);
            b.Property(x => x.IsActive).HasDefaultValue(true);

            b.Property(x => x.CreatedAt);
            b.Property(x => x.UpdatedAt);

            b.HasIndex(x => new { x.ParentId, x.Slug }).IsUnique();

            b.HasMany(x => x.Catalogs)
             .WithOne(x => x.Category)
             .HasForeignKey(x => x.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
