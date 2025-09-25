using CategoryApi.Biz.Model;
using CategoryApi.Biz.Model.Catalogs;
using CategoryApi.Data.Model.Entities;
using CategoryApi.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace CategoryApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogsController(ICatalogRepository repo) : ControllerBase
{
    static string NormalizeCode(string? code) => (code ?? "").Trim().ToUpperInvariant();

    static string Slugify(string? input)
    {
        var s = (input ?? "").Trim().ToLowerInvariant();
        s = s.Replace('đ', 'd').Replace('Đ', 'D'); // tiếng Việt
        s = s.Normalize(NormalizationForm.FormD);
        s = Regex.Replace(s, @"\p{IsCombiningDiacriticalMarks}+", "");
        s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
        s = Regex.Replace(s, @"\s+", "-");
        s = Regex.Replace(s, "-{2,}", "-").Trim('-');
        return string.IsNullOrWhiteSpace(s) ? "item" : s;
    }
    static string TrimSlug(string slug, int max = 220) => slug.Length <= max ? slug : slug[..max];

    async Task<string> EnsureUniqueSlugAsync(string rawSlug, int categoryId, int? excludeId, CancellationToken ct)
    {
        var baseSlug = TrimSlug(Slugify(rawSlug));
        var prefix = baseSlug + "-";

        var existing = await repo.Query()
            .Where(x => x.CategoryId == categoryId && (excludeId == null || x.Id != excludeId)
                     && (x.Slug == baseSlug || x.Slug.StartsWith(prefix)))
            .Select(x => x.Slug)
            .ToListAsync(ct);

        if (!existing.Contains(baseSlug)) return baseSlug;

        var maxN = 1;
        foreach (var s in existing)
            if (s.StartsWith(prefix) && int.TryParse(s[prefix.Length..], out var n))
                maxN = Math.Max(maxN, n);

        return TrimSlug($"{baseSlug}-{maxN + 1}");
    }

    [HttpGet]
    public async Task<PagedResult<CatalogList>> List([FromQuery] CatalogFilter f, CancellationToken ct)
    {
        var page = f.Page <= 0 ? 1 : f.Page;
        var size = f.Size <= 0 ? 10 : f.Size;

        var q = repo.Query();

        if (f.CategoryId.HasValue) q = q.Where(x => x.CategoryId == f.CategoryId);
        if (!string.IsNullOrWhiteSpace(f.Q))
        {
            var k = f.Q.Trim();
            q = q.Where(x => x.Name.Contains(k) || x.Slug.Contains(k) || x.Code.Contains(k));
        }
        if (f.MinPrice.HasValue) q = q.Where(x => x.Price >= f.MinPrice.Value);
        if (f.MaxPrice.HasValue) q = q.Where(x => x.Price <= f.MaxPrice.Value);
        if (f.IsActive.HasValue) q = q.Where(x => x.IsActive == f.IsActive.Value);

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
                CategoryName = x.Category != null ? x.Category.Name : null,
                Price = x.Price,
                IsActive = x.IsActive,
                ImageUrl = x.ImageUrl
            }).ToListAsync(ct);

        return new(page, size, total, items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CatalogView>> Get(int id, CancellationToken ct)
    {
        var x = await repo.Query().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (x is null) return NotFound();

        return new CatalogView
        {
            Id = x.Id,
            Name = x.Name,
            Code = x.Code,
            Slug = x.Slug,
            CategoryId = x.CategoryId,
            CategoryName = x.Category?.Name,
            Price = x.Price,
            ImageUrl = x.ImageUrl,
            Description = x.Description,
            IsActive = x.IsActive
        };
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CatalogNew model, CancellationToken ct)
    {
        if (!await repo.CategoryExistsAsync(model.CategoryId, ct))
            return NotFound($"Category {model.CategoryId} not found");

        var code = NormalizeCode(model.Code);
        if (await repo.CodeExistsAsync(code, null, ct))
            return Conflict("Code đã tồn tại");

        var slug = await EnsureUniqueSlugAsync(model.Name, model.CategoryId, null, ct);

        var e = new Catalog
        {
            Name = model.Name.Trim(),
            Code = code,
            Slug = slug,
            CategoryId = model.CategoryId,
            Price = model.Price < 0 ? 0 : model.Price,
            ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl.Trim(),
            Description = model.Description,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await repo.AddAsync(e, ct);
        await repo.SaveAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = e.Id }, e.Id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CatalogEdit model, CancellationToken ct)
    {
        var e = await repo.FindAsync(id, ct);
        if (e is null) return NotFound();

        if (!await repo.CategoryExistsAsync(model.CategoryId, ct))
            return NotFound($"Category {model.CategoryId} not found");

        var code = NormalizeCode(model.Code);
        if (await repo.CodeExistsAsync(code, id, ct))
            return Conflict("Code đã tồn tại");

        e.Name = model.Name.Trim();
        e.Code = code;
        e.CategoryId = model.CategoryId;
        e.Price = model.Price < 0 ? 0 : model.Price;
        e.ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl.Trim();
        e.Description = model.Description;
        e.IsActive = model.IsActive;
        e.UpdatedAt = DateTime.UtcNow;

        var newSlug = Slugify(e.Name);
        if (!newSlug.Equals(e.Slug, StringComparison.OrdinalIgnoreCase))
            e.Slug = await EnsureUniqueSlugAsync(newSlug, e.CategoryId, e.Id, ct);

        await repo.SaveAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var e = await repo.FindAsync(id, ct);
        if (e is null) return NoContent(); // idempotent

        await repo.RemoveAsync(e, ct);
        await repo.SaveAsync(ct);
        return NoContent();
    }
}
