// CategoryApi.Data/Repositories/ICatalogRepository.cs
using CategoryApi.Data.Model.Entities;

namespace CategoryApi.Data.Repositories;

public interface ICatalogRepository
{
    IQueryable<Catalog> Query(); // AsNoTracking cho list
    Task<Catalog?> FindAsync(int id, CancellationToken ct = default); // tracked để Update/Delete
    Task<bool> CategoryExistsAsync(int categoryId, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);

    Task AddAsync(Catalog e, CancellationToken ct = default);
    Task RemoveAsync(Catalog e, CancellationToken ct = default);
    Task<int> SaveAsync(CancellationToken ct = default);
}
