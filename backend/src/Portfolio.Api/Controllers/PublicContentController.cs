using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Infrastructure;

namespace Portfolio.Api.Controllers;

/// <summary>Read-only, cacheable endpoints consumed by the public Angular site (SSR/prerender).</summary>
[ApiController, Route("api/public")]
public class PublicContentController(PortfolioDbContext db) : ControllerBase
{
    /// <summary>All UI copy keyed by content key. Placeholder for the full content endpoints described in docs/IMPLEMENTATION_GUIDE.md.</summary>
    [HttpGet("content"), ResponseCache(Duration = 60)]
    public async Task<IActionResult> Content(CancellationToken ct)
    {
        var entries = await db.ContentEntries.AsNoTracking().ToListAsync(ct);
        return Ok(entries.ToDictionary(e => e.Key, e => new { en = e.Value.En, fa = e.Value.Fa }));
    }
}
