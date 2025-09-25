// CategoryApi.Api/Controllers/CategoriesController.cs
using CategoryApi.Biz.Model;
using CategoryApi.Biz.Model.Categories;
using CategoryApi.Data.Model.Entities;
using CategoryApi.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace CategoryApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ICategoryRepository repo) : ControllerBase
{
    // Helpers
    static string Slugify(string? input)
    {
        var s = (input ?? "").Trim().ToLowerInvariant();
        s = s.Replace('đ', 'd').Replace('Đ', 'D');          // tiếng Việt
        s = s.Normalize(NormalizationForm.FormD);
        s = Regex.Replace(s, @"\p{IsCombiningDiacriticalMarks}+", "");
        s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
        s = Regex.Replace(s, @"\s+", "-");
        s = Regex.Replace(s, "-{2,}", "-").Trim('-');
        if (string.IsNullOrWhiteSpace(s)) s = "category";
        return s.Length <= 220 ? s : s[..220];            // khớp max length cột Slug
    }

    async Task<string> EnsureUniqueSlugAsync(string rawSlug, int? parentId, int? excludeId, CancellationToken ct)
    {
        var baseSlug = Slugify(rawSlug);
        var slug = baseSlug; var i = 1;
        while (await repo.Query()
            .AnyAsync(x => x.ParentId == parentId && x.Slug == slug && x.Id != excludeId, ct))
        {
            slug = $"{baseSlug}-{++i}";
            if (slug.Length > 220) slug = slug[..220];
        }
        return slug;
    }

    // GET /api/categories
    [HttpGet]
    public async Task<PagedResult<CategoryList>> List([FromQuery] CategoryFilter filter, CancellationToken ct)
    {
        var page = filter.Page <= 0 ? 1 : filter.Page;
        var size = filter.Size <= 0 ? 10 : filter.Size;

        var q = repo.Query();

        if (!string.IsNullOrWhiteSpace(filter.Q))
        {
            var k = filter.Q.Trim();
            q = q.Where(x => x.Name.Contains(k) || x.Slug.Contains(k));
        }
        if (filter.ParentId.HasValue) q = q.Where(x => x.ParentId == filter.ParentId);

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

        return new(page, size, total, items);
    }

    // GET /api/categories/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryView>> Get(int id, CancellationToken ct)
    {
        var x = await repo.Query().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (x is null) return NotFound();

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

    // POST /api/categories
    [HttpPost]
    public async Task<ActionResult<int>> Create(CategoryNew model, CancellationToken ct)
    {
        if (model.ParentId.HasValue && !await repo.ExistsAsync(model.ParentId.Value, ct))
            return NotFound($"Parent {model.ParentId} not found");

        var slug = await EnsureUniqueSlugAsync(model.Name, model.ParentId, null, ct);

        var e = new Category
        {
            Name = (model.Name ?? "").Trim(),
            Slug = slug,
            ParentId = model.ParentId,
            SortOrder = model.SortOrder,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await repo.AddAsync(e, ct);
        await repo.SaveAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = e.Id }, e.Id);
    }

    // PUT /api/categories/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CategoryEdit model, CancellationToken ct)
    {
        var e = await repo.FindAsync(id, ct);
        if (e is null) return NotFound();

        if (model.ParentId == id) return BadRequest("ParentId không được trỏ tới chính nó.");
        if (model.ParentId.HasValue && !await repo.ExistsAsync(model.ParentId.Value, ct))
            return NotFound($"Parent {model.ParentId} not found");

        e.Name = (model.Name ?? "").Trim();
        e.ParentId = model.ParentId;
        e.SortOrder = model.SortOrder;
        e.IsActive = model.IsActive;
        e.UpdatedAt = DateTime.UtcNow;

        var newSlug = Slugify(e.Name);
        if (!newSlug.Equals(e.Slug, StringComparison.OrdinalIgnoreCase))
            e.Slug = await EnsureUniqueSlugAsync(newSlug, e.ParentId, e.Id, ct);

        await repo.SaveAsync(ct);
        return NoContent();
    }

    // DELETE /api/categories/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var e = await repo.FindAsync(id, ct);
        if (e is null) return NoContent(); // idempotent

        if (await repo.HasChildrenAsync(id, ct))
            return Conflict("Không thể xoá: đang có category con.");

        if (await repo.InUseByCatalogsAsync(id, ct))
            return Conflict("Không thể xoá: đang được dùng bởi Catalogs.");

        await repo.RemoveAsync(e, ct);
        await repo.SaveAsync(ct);
        return NoContent();
    }
}
