using CategoryApi.Biz.Model;
using CategoryApi.Biz.Model.Catalogs;

namespace CategoryApi.Biz.Services;

public interface ICatalogService
{
    Task<PagedResult<CatalogList>> ListAsync(CatalogFilter filter, CancellationToken ct = default);
    Task<CatalogView?> GetAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(CatalogNew model, CancellationToken ct = default);
    Task UpdateAsync(int id, CatalogEdit model, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
