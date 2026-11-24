using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Dtos;
using Portfolio.Infrastructure;

namespace Portfolio.Api.Controllers.Admin;

/// <summary>Reorder and show/hide page sections.</summary>
[ApiController, Route("api/admin/sections"), Authorize(Roles = "Admin")]
public class AdminSectionsController(PortfolioDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<AdminSectionDto>> List(CancellationToken ct) =>
        await db.Sections.AsNoTracking().OrderBy(s => s.SortOrder)
            .Select(s => new AdminSectionDto(s.Key, s.SortOrder, s.IsVisible)).ToListAsync(ct);

    /// <summary>Send every section in the desired order; the position in the list becomes its SortOrder.</summary>
    [HttpPut]
    public async Task<ActionResult<List<AdminSectionDto>>> Save(List<SectionOrderItem> items, CancellationToken ct)
    {
        var sections = await db.Sections.ToListAsync(ct);
        if (items.Count != sections.Count || items.Select(i => i.Key).Distinct().Count() != items.Count
            || items.Any(i => sections.All(s => s.Key != i.Key)))
            return BadRequest(new { error = "Send each existing section exactly once." });

        for (var i = 0; i < items.Count; i++)
        {
            var s = sections.First(x => x.Key == items[i].Key);
            s.SortOrder = i;
            s.IsVisible = items[i].IsVisible;
        }
        await db.SaveChangesAsync(ct);
        return await List(ct);
    }
}
