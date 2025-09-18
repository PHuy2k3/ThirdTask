using CategoryApi.Data.Model.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace CategoryApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> option) : DbContext(option)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Catalog> Catalogs => Set<Catalog>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<Category>(e =>
        {
            e.HasIndex(x => new { x.ParentId, x.Slug }).IsUnique();
        });

        b.Entity<Catalog>(e =>
        {
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => new { x.CategoryId, x.Slug }).IsUnique();
            e.Property(x => x.ImageUrl).HasMaxLength(1024);

            e.HasOne(x => x.Category)
            .WithMany(c => c.Catalogs)
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
