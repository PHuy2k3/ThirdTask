using CategoryApi.Biz.Model;
using CategoryApi.Biz.Model.Categories;
using CategoryApi.Data;
using CategoryApi.Data.Model.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace CategoryApi.Biz.Services;

public class CategoryService(AppDbContext db) : ICategoryService
{
    private readonly AppDbContext _db = db;

    static string Slugify(string input)
    {
        input ??= "";
        var s = input.Trim().ToLowerInvariant();
        s = s.Normalize(NormalizationForm.FormD);
        s = Regex.Replace(s, @"\p{IsCombiningDiacriticalMarks}+", "");
        s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
        s = Regex.Replace(s, @"\s+", "-");
        s = Regex.Replace(s, "-{2,}", "-").Trim('-');
        return string.IsNullOrWhiteSpace(s) ? "category" : s;
    }

    async Task<string> EnsureUniqueSlugAsync(string slug, int? excludeId = null, CancellationToken ct = default)
    {
        var baseSlug = slug; int i = 1;
        while (await _db.Categories.AnyAsync(x => x.Slug == slug && x.Id != excludeId, ct))
            slug = $"{baseSlug}-{++i}";
        return slug;
    }

    public async Task<PagedResult<CategoryList>> ListAsync(CategoryFilter filter, CancellationToken ct = default)
    {
        var page = filter.Page <= 0 ? 1 : filter.Page;
        var size = filter.Size <= 0 ? 10 : filter.Size;

        var q = _db.Categories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Q))
        {
            var k = filter.Q.Trim();
            q = q.Where(x => x.Name.Contains(k) || x.Slug.Contains(k));
        }
        if (filter.ParentId.HasValue)
            q = q.Where(x => x.ParentId == filter.ParentId);

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
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
        var x = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
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

    public async Task<int> CreateAsync(CategoryNew m, CancellationToken ct = default)
    {
        var slug = await EnsureUniqueSlugAsync(Slugify(m.Name), null, ct);

        var e = new Category
        {
            Name = m.Name,
            Slug = slug,
            ParentId = m.ParentId,
            SortOrder = m.SortOrder,
            IsActive = m.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Categories.Add(e);
        await _db.SaveChangesAsync(ct);
        return e.Id;
    }

    public async Task UpdateAsync(int id, CategoryEdit m, CancellationToken ct = default)
    {
        var e = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Category {id} not found");

        e.Name = m.Name;
        e.ParentId = m.ParentId;
        e.SortOrder = m.SortOrder;
        e.IsActive = m.IsActive;
        e.UpdatedAt = DateTime.UtcNow;

        var newSlug = Slugify(m.Name);
        if (!string.Equals(newSlug, e.Slug, StringComparison.OrdinalIgnoreCase))
            e.Slug = await EnsureUniqueSlugAsync(newSlug, e.Id, ct);

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Categories.FindAsync(new object[] { id }, ct);
        if (e is null) return;
        _db.Categories.Remove(e);
        await _db.SaveChangesAsync(ct);
    }
}
