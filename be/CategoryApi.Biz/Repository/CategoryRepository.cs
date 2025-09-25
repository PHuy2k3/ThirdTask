// CategoryApi.Data/Repositories/CategoryRepository.cs
using CategoryApi.Data.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace CategoryApi.Data.Repositories;

public sealed class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public IQueryable<Category> Query()
        => db.Categories.AsNoTracking();

    public Task<Category?> FindAsync(int id, CancellationToken ct = default)
        => db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct); // tracked

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => db.Categories.AnyAsync(x => x.Id == id, ct);

    public Task<bool> HasChildrenAsync(int id, CancellationToken ct = default)
        => db.Categories.AnyAsync(x => x.ParentId == id, ct);

    public Task<bool> InUseByCatalogsAsync(int id, CancellationToken ct = default)
        => db.Catalogs.AnyAsync(x => x.CategoryId == id, ct);

    public Task AddAsync(Category e, CancellationToken ct = default)
    { db.Categories.Add(e); return Task.CompletedTask; }

    public Task RemoveAsync(Category e, CancellationToken ct = default)
    { db.Categories.Remove(e); return Task.CompletedTask; }

    public Task<int> SaveAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
