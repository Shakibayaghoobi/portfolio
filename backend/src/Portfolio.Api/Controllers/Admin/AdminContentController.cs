using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Dtos;
using Portfolio.Infrastructure;

namespace Portfolio.Api.Controllers.Admin;

/// <summary>Edit site copy (the "Texts" screen). Keys are fixed by the seed; admin changes the EN/FA values.</summary>
[ApiController, Route("api/admin/content"), Authorize(Roles = "Admin")]
public class AdminContentController(PortfolioDbContext db) : ControllerBase
{
    public const int MaxLength = 4000;

    [HttpGet]
    public async Task<List<AdminContentEntryDto>> List([FromQuery] string? group, CancellationToken ct)
    {
        var q = db.ContentEntries.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(group)) q = q.Where(e => e.Group == group);
        var items = await q.OrderBy(e => e.Group).ThenBy(e => e.Id).ToListAsync(ct);
        return items.Select(e => new AdminContentEntryDto(e.Id, e.Key, e.Group, e.Value.ToDto())).ToList();
    }

    [HttpPut("{key}")]
    public async Task<ActionResult<AdminContentEntryDto>> Update(string key, LocalizedDto value, CancellationToken ct)
    {
        if (value.En.Length > MaxLength || value.Fa.Length > MaxLength)
            return BadRequest(new { error = $"Text is limited to {MaxLength} characters." });
        var entry = await db.ContentEntries.FirstOrDefaultAsync(e => e.Key == key, ct);
        if (entry is null) return NotFound();
        entry.Value.En = value.En;
        entry.Value.Fa = value.Fa;
        await db.SaveChangesAsync(ct);
        return new AdminContentEntryDto(entry.Id, entry.Key, entry.Group, entry.Value.ToDto());
    }
}
