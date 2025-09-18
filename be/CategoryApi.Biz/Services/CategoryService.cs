using CategoryApi.Biz.Model;
using CategoryApi.Biz.Model.Categories;
using CategoryApi.Common;
using CategoryApi.Data;
using CategoryApi.Data.Model.Entities;
using CategoryApi.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CategoryApi.Biz.Services;

public class CategoryService(IRepository<Category> repo, AppDbContext db): ICategoryService
{
    private readonly IRepository<Category> _repo = repo;
    private readonly AppDbContext _db = db;
    private static string Slugify(string input)
    {
        input ??= "";
        var s = input.Trim().ToLowerInvariant();
        s = s.Normalize(NormalizationForm.FormD);
        s = Regex.Replace(s, @"\p{IsCombiningDiacriticalMarks}+", "");
        s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
        s = Regex.Replace(s, @"\s+", "-");
        s = Regex.Replace(s, "-{2,}", "-").Trim('-');
        return string.IsNullOrWhiteSpace(s) ? "cat" : s;
    }
    private async Task<string> EnsureUniqueSlugAsync(string slug, int? parentId, int? excludeId = null, CancellationToken ct = default)
    {
        var baseSlug = slug;
        int i = 1;
        while (await _db.Categories.AnyAsync(x => x.ParentId == parentId && x.Slug == slug && x.Id != excludeId, ct))
            slug = $"{baseSlug}-{++i}";
        return slug;
    }

    public async Task<PagedResult<CategoryList>> ListAsync(CategoryFilter filter, CancellationToken ct = default)
    {
        var page = filter.Page <= 0 ? 1 : filter.Page;
        var size = filter.Size <= 0 ? 10 :filter.Size;
        var q = _repo.Query().AsNoTracking();

        if(filter.ParentId.HasValue) q = q.Where(x => x.ParentId == filter.ParentId);
        if (!string.IsNullOrWhiteSpace(filter.Q))
        {
            var k = filter.Q.Trim();
            q = q.Where(x => x.Name.Contains(k) || x.Slug.Contains(k));
        }
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(x => x.ParentId).ThenBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Skip((page - 1) * size).Take(size)
            .Select(x => new CategoryList
            {
                Id = x.Id, 
                Name = x.Name, 
                Slug = x.Slug,
                ParentId = x.ParentId, 
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            }).ToListAsync(ct);
        return new PagedResult<CategoryList>(page, size, total, items);
    }

    public async Task<CategoryView?> GetAsync(int id, CancellationToken ct = default)
    {
        var x = await _repo.Query().AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (x is null) return null;
        return new CategoryView
        {
            Id = x.Id,
            Name = x.Name,
            Slug = x.Slug,
            ParentId = x.ParentId,
            SortOrder = x.SortOrder,
            IsActive = x.IsActive
        };
    }

    public async Task<int> CreateAsync(CategoryNew model, CancellationToken ct = default)
    {
        var slug = await EnsureUniqueSlugAsync(Slugify(model.Name), model.ParentId, null, ct);
        var e = new Category
        {
            Name = model.Name,
            Slug = slug,
            ParentId = model.ParentId,
            SortOrder = model.SortOrder,
            IsActive= model.IsActive
        };
        await _repo.AddAsync(e, ct);
        await _repo.SaveChangesAsync(ct);
        return e.Id;
    }

    public async Task UpdateAsync(int id, CategoryEdit model, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException($"Category {id} not found");
        e.Name = model.Name; e.ParentId = model.ParentId; e.SortOrder = model.SortOrder;
        e.IsActive = model.IsActive;

        var newSlug = Slugify(model.Name);
        if (!string.Equals(newSlug, e.Slug, StringComparison.OrdinalIgnoreCase))
            e.Slug = await EnsureUniqueSlugAsync(newSlug, e.ParentId, e.Id, ct);

        _repo.Update(e);
        await _repo.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var hasChildren = await _db.Categories.AnyAsync(x => x.ParentId == id, ct);
        if (hasChildren) throw new InvalidOperationException("Không thể xoá do còn danh mục con.");
        var e = await _repo.GetByIdAsync(id, ct);
        if (e is null) return;
        _repo.Remove(e);
        await _repo.SaveChangesAsync(ct);
    }
}
