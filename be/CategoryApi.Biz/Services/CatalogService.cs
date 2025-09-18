using CategoryApi.Biz.Model;
using CategoryApi.Biz.Model.Catalogs;
using CategoryApi.Data;
using CategoryApi.Data.Model.Entities;
using CategoryApi.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace CategoryApi.Biz.Services;

public class CatalogService(IRepository<Catalog> repo, AppDbContext db) : ICatalogService
{
    private readonly IRepository<Catalog> _repo = repo;
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
        return string.IsNullOrWhiteSpace(s) ? "item" : s;
    }

    async Task<string> EnsureUniqueSlugAsync(string slug, int categoryId, int? excludeId = null, CancellationToken ct = default)
    {
        var baseSlug = slug; int i = 1;
        while (await _db.Catalogs.AnyAsync(x => x.CategoryId == categoryId && x.Slug == slug && x.Id != excludeId, ct))
            slug = $"{baseSlug}-{++i}";
        return slug;
    }

    public async Task<PagedResult<CatalogList>> ListAsync(CatalogFilter filter, CancellationToken ct = default)
    {
        var page = filter.Page <= 0 ? 1 : filter.Page;
        var size = filter.Size <= 0 ? 10 : filter.Size;

        var q = _repo.Query().AsNoTracking();

        if (filter.CategoryId.HasValue) q = q.Where(x => x.CategoryId == filter.CategoryId);
        if (!string.IsNullOrWhiteSpace(filter.Q))
        {
            var k = filter.Q.Trim();
            q = q.Where(x => x.Name.Contains(k) || x.Slug.Contains(k) || x.Code.Contains(k));
        }
        if (filter.MinPrice.HasValue) q = q.Where(x => x.Price >= filter.MinPrice.Value);
        if (filter.MaxPrice.HasValue) q = q.Where(x => x.Price <= filter.MaxPrice.Value);
        if (filter.IsActive.HasValue) q = q.Where(x => x.IsActive == filter.IsActive.Value);

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(x => x.CategoryId).ThenBy(x => x.Name)
            .Skip((page - 1) * size).Take(size)
            .Select(x => new CatalogList
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                Slug = x.Slug,
                CategoryId = x.CategoryId,
                Price = x.Price,
                IsActive = x.IsActive, 
                ImageUrl = x.ImageUrl
            }).ToListAsync(ct);

        return new PagedResult<CatalogList>(page, size, total, items);
    }

    public async Task<CatalogView?> GetAsync(int id, CancellationToken ct = default)
    {
        var x = await _repo.Query().AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (x is null) return null;
        return new CatalogView
        {
            Id = x.Id,
            Name = x.Name,
            Code = x.Code,
            Slug = x.Slug,
            CategoryId = x.CategoryId,
            Price = x.Price,
            ImageUrl = x.ImageUrl,
            Description = x.Description,
            IsActive = x.IsActive  
        };
    }

    public async Task<int> CreateAsync(CatalogNew m, CancellationToken ct = default)
    {
        if (!await _db.Categories.AnyAsync(c => c.Id == m.CategoryId, ct))
            throw new KeyNotFoundException($"Category {m.CategoryId} not found");

        if (await _db.Catalogs.AnyAsync(x => x.Code == m.Code, ct))
            throw new InvalidOperationException("Code đã tồn tại");

        var slug = await EnsureUniqueSlugAsync(Slugify(m.Name), m.CategoryId, null, ct);

        var e = new Catalog
        {
            Name = m.Name,
            Code = m.Code,
            Slug = slug,
            CategoryId = m.CategoryId,
            Price = m.Price,
            ImageUrl = string.IsNullOrWhiteSpace(m.ImageUrl) ? null : m.ImageUrl.Trim(),
            Description = m.Description,
            IsActive = m.IsActive,      
            CreatedAt = DateTime.UtcNow 
        };
        await _repo.AddAsync(e, ct);
        await _repo.SaveChangesAsync(ct);
        return e.Id;
    }

    public async Task UpdateAsync(int id, CatalogEdit m, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException($"Catalog {id} not found");

        if (!await _db.Categories.AnyAsync(c => c.Id == m.CategoryId, ct))
            throw new KeyNotFoundException($"Category {m.CategoryId} not found");

        if (await _db.Catalogs.AnyAsync(x => x.Id != id && x.Code == m.Code, ct))
            throw new InvalidOperationException("Code đã tồn tại");

        e.Name = m.Name;
        e.Code = m.Code;
        e.CategoryId = m.CategoryId;
        e.Price = m.Price;
        e.ImageUrl =string.IsNullOrWhiteSpace(m.ImageUrl) ? null : m.ImageUrl.Trim();
        e.Description = m.Description;
        e.IsActive = m.IsActive;          
        e.UpdatedAt = DateTime.UtcNow;  

        var newSlug = Slugify(m.Name);
        if (!string.Equals(newSlug, e.Slug, StringComparison.OrdinalIgnoreCase))
            e.Slug = await EnsureUniqueSlugAsync(newSlug, e.CategoryId, e.Id, ct);

        _repo.Update(e);
        await _repo.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdAsync(id, ct);
        if (e is null) return;
        _repo.Remove(e);
        await _repo.SaveChangesAsync(ct);
    }
}
