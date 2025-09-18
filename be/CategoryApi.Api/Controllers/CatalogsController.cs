using CategoryApi.Biz.Model;
using CategoryApi.Biz.Model.Catalogs;
using CategoryApi.Biz.Services;
using Microsoft.AspNetCore.Mvc;

namespace CategoryApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogsController(ICatalogService service) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<CatalogList>> List([FromQuery] CatalogFilter filter, CancellationToken ct)
        => service.ListAsync(filter, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CatalogView>> Get(int id, CancellationToken ct)
        => (await service.GetAsync(id, ct)) is { } x ? x : NotFound();

    [HttpPost]
    public async Task<ActionResult<int>> Create(CatalogNew model, CancellationToken ct)
    {
        var id = await service.CreateAsync(model, ct);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CatalogEdit model, CancellationToken ct)
    { await service.UpdateAsync(id, model, ct); return NoContent(); }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    { await service.DeleteAsync(id, ct); return NoContent(); }
}
