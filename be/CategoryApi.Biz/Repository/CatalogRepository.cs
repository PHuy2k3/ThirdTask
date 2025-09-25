// CategoryApi.Data/Repositories/CatalogRepository.cs
using CategoryApi.Data.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace CategoryApi.Data.Repositories;

public sealed class CatalogRepository(AppDbContext db) : ICatalogRepository
{
    public IQueryable<Catalog> Query()
        => db.Catalogs.AsNoTracking().Include(x => x.Category);

    public Task<Catalog?> FindAsync(int id, CancellationToken ct = default)
        => db.Catalogs.FirstOrDefaultAsync(x => x.Id == id, ct); // tracked

    public Task<bool> CategoryExistsAsync(int categoryId, CancellationToken ct = default)
        => db.Categories.AnyAsync(x => x.Id == categoryId, ct);

    public Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default)
        => db.Catalogs.AnyAsync(x => x.Code == code && (excludeId == null || x.Id != excludeId), ct);

    public Task AddAsync(Catalog e, CancellationToken ct = default)
    { db.Catalogs.Add(e); return Task.CompletedTask; }

    public Task RemoveAsync(Catalog e, CancellationToken ct = default)
    { db.Catalogs.Remove(e); return Task.CompletedTask; }

    public Task<int> SaveAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
