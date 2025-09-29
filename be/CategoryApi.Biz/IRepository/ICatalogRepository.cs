using CategoryApi.Data.Model.Entities;

namespace CategoryApi.Data.Repositories;

public interface ICatalogRepository
{
    IQueryable<Catalog> Query();
    Task<Catalog?> FindAsync(int id, CancellationToken ct = default);
    Task<bool> CategoryExistsAsync(int categoryId, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);

    Task AddAsync(Catalog e, CancellationToken ct = default);
    Task RemoveAsync(Catalog e, CancellationToken ct = default);
    Task<int> SaveAsync(CancellationToken ct = default);
}
cd 