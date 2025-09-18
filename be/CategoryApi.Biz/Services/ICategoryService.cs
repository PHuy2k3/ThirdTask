using CategoryApi.Biz.Model;
using CategoryApi.Biz.Model.Categories;

namespace CategoryApi.Biz.Services;

public interface ICategoryService
{
    Task<PagedResult<CategoryList>> ListAsync(CategoryFilter filter, CancellationToken ct = default);
    Task<CategoryView?> GetAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(CategoryNew model, CancellationToken ct = default);
    Task UpdateAsync(int id, CategoryEdit model, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
