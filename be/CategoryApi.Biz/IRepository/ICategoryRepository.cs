// CategoryApi.Data/Repositories/ICategoryRepository.cs
using CategoryApi.Data.Model.Entities;

namespace CategoryApi.Data.Repositories;

public interface ICategoryRepository
{
    IQueryable<Category> Query(); // AsNoTracking cho list
    Task<Category?> FindAsync(int id, CancellationToken ct = default); // tracked cho update/delete
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task<bool> HasChildrenAsync(int id, CancellationToken ct = default);
    Task<bool> InUseByCatalogsAsync(int id, CancellationToken ct = default);

    Task AddAsync(Category e, CancellationToken ct = default);
    Task RemoveAsync(Category e, CancellationToken ct = default);
    Task<int> SaveAsync(CancellationToken ct = default);
}
